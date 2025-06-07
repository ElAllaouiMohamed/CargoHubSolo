import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout
import os


class TestItemGroupsEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/itemgroups/"
        timeout = Timeout(60.0)  # 60timout
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.client = Client(timeout=timeout)
        self.client.headers = {
            "X-Api-Key": api_key,
            "Content-Type": "application/json",
        }
        self.test_id = None  # Will store ID after creation

        self.test_item_group = {
            "name": "Test Group",
            "description": "Test item group description",
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False,
        }

        self.updated_item_group = {
            "name": "Updated Group",
            "description": "Updated description",
            "created_at": self.test_item_group["created_at"],
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False,
        }

        # Clean up any existing test data
        response = self.client.get(self.base_url)
        if response.status_code == 200:
            item_groups = response.json()
            for item_group in item_groups:
                if item_group["name"] in ["Test Group", "Updated Group"]:
                    self.client.delete(f"{self.base_url}{item_group['id']}")

    def tearDown(self):
        # Clean up test data
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_post_item_group(self):
        response = self.client.post(self.base_url, json=self.test_item_group)
        self.assertIn(
            response.status_code,
            (200, 201),
            f"Failed to create item group: {response.text}",
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id, "Item group ID should be returned")
        self.assertEqual(data["name"], self.test_item_group["name"])
        self.assertEqual(data["description"], self.test_item_group["description"])
        self.assertFalse(data["isDeleted"])

    def test_2_get_item_group(self):
        if not self.test_id:
            self.skipTest("Create item group test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(
            response.status_code, 200, f"Failed to get item group: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["name"], self.test_item_group["name"])
        self.assertEqual(data["description"], self.test_item_group["description"])

    def test_3_update_item_group(self):
        if not self.test_id:
            self.skipTest("Create item group test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_item_group
        )
        self.assertEqual(
            response.status_code, 200, f"Failed to update item group: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["name"], self.updated_item_group["name"])
        self.assertEqual(data["description"], self.updated_item_group["description"])
        self.assertFalse(data["isDeleted"])

    def test_4_get_all_item_groups(self):
        response = self.client.get(self.base_url)
        self.assertEqual(
            response.status_code, 200, f"Failed to get all item groups: {response.text}"
        )
        data = response.json()
        self.assertTrue(isinstance(data, list))
        if self.test_id:
            found = any(item["id"] == self.test_id for item in data)
            self.assertTrue(found, "Created item group should be in the list")

    def test_5_soft_delete_item_group(self):
        if not self.test_id:
            self.skipTest("Create item group test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(
            response.status_code,
            204,
            f"Failed to soft delete item group: {response.text}",
        )

        # Check that deleted item group no longer returns 200
        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(
            get_response.status_code, 404, "Soft-deleted item group should return 404"
        )

    def test_6_post_invalid_name(self):
        invalid_item_group = {
            "name": "Invalid123",  # Violates regex
            "description": "Invalid name test",
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False,
        }
        response = self.client.post(self.base_url, json=invalid_item_group)
        self.assertEqual(
            response.status_code, 400, f"Expected 400 for invalid name: {response.text}"
        )
        self.assertIn("Numbers and special characters are not allowed", response.text)


if __name__ == "__main__":
    unittest.main()
