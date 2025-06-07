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

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_valid_item_group_creation(self):
        response = self.client.post(self.base_url, json=self.valid_item_group)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(data["name"], self.valid_item_group["name"])

    def test_2_invalid_name_validation(self):
        response = self.client.post(self.base_url, json=self.invalid_item_group)
        self.assertEqual(response.status_code, 400)
        self.assertIn("Numbers and special characters are not allowed", response.text)

    def test_3_missing_name_field(self):
        response = self.client.post(self.base_url, json=self.missing_name_group)
        self.assertEqual(response.status_code, 400)
        self.assertIn("Name is verplicht", response.text)


if __name__ == "__main__":
    unittest.main()
