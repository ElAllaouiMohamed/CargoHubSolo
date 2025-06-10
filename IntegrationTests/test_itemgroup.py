import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestItemGroupsEndpoint(unittest.TestCase):
    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/itemgroups/"
        self.client = Client(
            timeout=Timeout(60.0),
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.test_id = None

        now = datetime.now(timezone.utc).isoformat()

        self.valid_item_group = {
            "name": "Valid Group",
            "description": "A valid item group",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

        self.updated_item_group = {
            "name": "Updated Group",
            "description": "Updated item group description",
            "created_at": now,
            "updated_at": datetime.now(timezone.utc).isoformat(),
            "isDeleted": False,
        }

        self.invalid_item_group = {
            "name": "Inval!d123",
            "description": "Invalid due to characters",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

        self.missing_name_group = {
            "description": "Missing name field",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

    def test_1_valid_item_group_creation(self):
        response = self.client.post(self.base_url, json=self.valid_item_group)
        self.assertIn(
            response.status_code, [200, 201], f"Creation failed: {response.text}"
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(data["name"], self.valid_item_group["name"])

    def test_2_invalid_name_validation(self):
        response = self.client.post(self.base_url, json=self.invalid_item_group)
        self.assertIn(
            response.status_code,
            [400, 422],
            f"Expected validation error: {response.text}",
        )
        data = response.json()
        self.assertIn("errors", data, "Expected validation errors in response")
        errors = str(data["errors"].values())
        self.assertTrue(
            "Numbers and special characters are not allowed" in errors,
            "Expected 'Numbers and special characters are not allowed' error message",
        )

    def test_3_missing_name_field(self):
        response = self.client.post(self.base_url, json=self.missing_name_group)
        self.assertIn(
            response.status_code,
            [400, 422],
            f"Expected validation error: {response.text}",
        )
        data = response.json()
        self.assertIn("errors", data, "Expected validation errors in response")
        errors = str(data["errors"].values())
        self.assertTrue(
            "Name is verplicht" in errors, "Expected 'Name is verplicht' error message"
        )

    def test_4_get_item_group(self):
        response = self.client.post(self.base_url, json=self.valid_item_group)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["name"], self.valid_item_group["name"])

    def test_5_update_item_group(self):
        response = self.client.post(self.base_url, json=self.valid_item_group)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_item_group
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["name"], self.updated_item_group["name"])
        self.assertEqual(data["description"], self.updated_item_group["description"])

    def test_6_get_all_item_groups(self):
        response = self.client.post(self.base_url, json=self.valid_item_group)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertTrue(isinstance(data, list))
        found = any(item["id"] == self.test_id for item in data)
        self.assertTrue(found, "Created item group should be in the list")

    def test_7_soft_delete_item_group(self):
        response = self.client.post(self.base_url, json=self.valid_item_group)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertIn(response.status_code, [200, 204])

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertIn(get_response.status_code, [404, 410])

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()


if __name__ == "__main__":
    unittest.main()
