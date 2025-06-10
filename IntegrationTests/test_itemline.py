import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestItemLinesEndpoint(unittest.TestCase):

    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/itemlines/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.test_id = None

        now = datetime.now(timezone.utc).isoformat()

        self.test_itemline = {
            "name": "Test Line",
            "description": "Test item line description",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

        self.updated_itemline = {
            "name": "Updated Line",
            "description": "Updated description",
            "created_at": now,
            "updated_at": datetime.now(timezone.utc).isoformat(),
            "isDeleted": False,
        }

        self.invalid_itemline = {
            "name": "Invalid123!",
            "description": "Invalid item line description",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

        self.invalid_itemline_missing_fields = {
            "description": "Mist name description",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

    def test_1_post_itemline(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201), f"POST failed: {response.text}")
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(data["name"], self.test_itemline["name"])

    def test_2_post_invalid_name(self):
        response = self.client.post(self.base_url, json=self.invalid_itemline)
        self.assertIn(
            response.status_code,
            (400, 422),
            f"Expected validation error: {response.text}",
        )
        data = response.json()
        self.assertIn("errors", data, "Expected validation errors in response")
        errors = str(data["errors"].values())
        self.assertTrue(
            "Name mag geen cijfers of speciale tekens bevatten" in errors,
            "Expected 'Name mag geen cijfers of speciale tekens bevatten' error message",
        )

    def test_3_post_missing_name(self):
        response = self.client.post(
            self.base_url, json=self.invalid_itemline_missing_fields
        )
        self.assertIn(
            response.status_code,
            (400, 422),
            f"Expected validation error: {response.text}",
        )
        data = response.json()
        self.assertIn("errors", data, "Expected validation errors in response")
        errors = str(data["errors"].values())
        self.assertTrue(
            "name is verplicht" in errors.lower(),
            "Expected 'name is verplicht' error message",
        )

    def test_4_get_itemline(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["name"], self.test_itemline["name"])

    def test_5_update_itemline(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_itemline
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["name"], self.updated_itemline["name"])
        self.assertEqual(data["description"], self.updated_itemline["description"])

    def test_6_get_all_itemlines(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertTrue(isinstance(data, list))
        found = any(item["id"] == self.test_id for item in data)
        self.assertTrue(found, "Created item line should be in the list")

    def test_7_soft_delete_itemline(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertIn(response.status_code, (200, 204))

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertIn(get_response.status_code, (404, 410))

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()


if __name__ == "__main__":
    unittest.main()
