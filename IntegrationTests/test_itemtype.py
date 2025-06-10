import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestItemTypesEndpoint(unittest.TestCase):
    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/itemtypes/"
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

        self.test_item_type = {
            "Name": "Test Type",
            "Description": "Test item type description",
            "CreatedAt": now,
            "UpdatedAt": now,
            "IsDeleted": False,
        }

        self.updated_item_type = {
            "Name": "Updated Type",
            "Description": "Updated description",
            "CreatedAt": now,
            "UpdatedAt": datetime.now(timezone.utc).isoformat(),
            "IsDeleted": False,
        }

        self.invalid_item_type = {
            "Description": "Invalid item type description",
            "CreatedAt": now,
            "UpdatedAt": now,
            "IsDeleted": False,
        }

        # Cleanup existing test data
        response = self.client.get(self.base_url)
        if response.status_code == 200:
            for entry in response.json():
                if entry["name"] in ["Test Type", "Updated Type"]:
                    self.client.delete(f"{self.base_url}{entry['id']}")

    def test_1_create_item_type(self):
        response = self.client.post(self.base_url, json=self.test_item_type)
        self.assertIn(
            response.status_code, (200, 201), f"Creation failed: {response.text}"
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(data.get("name"), self.test_item_type["Name"])

    def test_2_create_invalid_item_type_should_fail(self):
        response = self.client.post(self.base_url, json=self.invalid_item_type)
        self.assertIn(
            response.status_code,
            (400, 422),
            f"Expected validation error: {response.text}",
        )
        data = response.json()
        self.assertIn("errors", data, "Expected validation errors in response")
        errors = str(data["errors"].values())
        self.assertTrue(
            "Name is verplicht" in errors, "Expected 'Name is verplicht' error message"
        )

    def test_3_get_item_type(self):
        response = self.client.post(self.base_url, json=self.test_item_type)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["name"], self.test_item_type["Name"])
        self.assertEqual(data["description"], self.test_item_type["Description"])

    def test_4_update_item_type(self):
        response = self.client.post(self.base_url, json=self.test_item_type)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_item_type
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["name"], self.updated_item_type["Name"])
        self.assertEqual(data["description"], self.updated_item_type["Description"])
        self.assertFalse(data["isDeleted"])

    def test_5_get_all_item_types(self):
        response = self.client.post(self.base_url, json=self.test_item_type)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertTrue(isinstance(data, list))
        found = any(item["id"] == self.test_id for item in data)
        self.assertTrue(found, "Created item type should be in the list")

    def test_6_soft_delete_item_type(self):
        response = self.client.post(self.base_url, json=self.test_item_type)
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
