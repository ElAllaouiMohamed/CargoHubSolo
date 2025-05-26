import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout
class TestItemTypesEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/itemtypes/"
        timeout = Timeout(60.0)  
        self.client = Client(timeout=timeout)
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json"
        }
        self.test_id = None

        self.test_item_type = {
            "name": "Test Type",
            "description": "Test item type description",
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False
        }

        self.updated_item_type = {
            "name": "Updated Type",
            "description": "Updated description",
            "created_at": self.test_item_type["created_at"],
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False
        }

        response = self.client.get(self.base_url)
        if response.status_code == 200:
            item_types = response.json()
            for item_type in item_types:
                if item_type["name"] in ["Test Type", "Updated Type"]:
                    self.client.delete(f"{self.base_url}{item_type['id']}")

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_post_item_type(self):
        response = self.client.post(self.base_url, json=self.test_item_type)
        self.assertIn(response.status_code, (200, 201), f"Failed to create item type: {response.text}")
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id, "Item type ID should be returned")
        self.assertEqual(data["name"], self.test_item_type["name"])
        self.assertEqual(data["description"], self.test_item_type["description"])
        self.assertFalse(data["isDeleted"])

    def test_2_get_item_type(self):
        if not self.test_id:
            self.skipTest("Create item type test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200, f"Failed to get item type: {response.text}")
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["name"], self.test_item_type["name"])
        self.assertEqual(data["description"], self.test_item_type["description"])

    def test_3_update_item_type(self):
        if not self.test_id:
            self.skipTest("Create item type test failed or not run.")

        response = self.client.put(f"{self.base_url}{self.test_id}", json=self.updated_item_type)
        self.assertEqual(response.status_code, 200, f"Failed to update item type: {response.text}")
        data = response.json()
        self.assertEqual(data["name"], self.updated_item_type["name"])
        self.assertEqual(data["description"], self.updated_item_type["description"])
        self.assertFalse(data["isDeleted"])

    def test_4_get_all_item_types(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200, f"Failed to get all item types: {response.text}")
        data = response.json()
        self.assertTrue(isinstance(data, list))
        if self.test_id:
            found = any(item["id"] == self.test_id for item in data)
            self.assertTrue(found, "Created item type should be in the list")

    def test_5_soft_delete_item_type(self):
        if not self.test_id:
            self.skipTest("Create item type test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 204, f"Failed to soft delete item type: {response.text}")

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(get_response.status_code, 404, "Soft-deleted item type should return 404")

if __name__ == "__main__":
    unittest.main()