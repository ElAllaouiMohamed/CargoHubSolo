import unittest
from httpx import Client, Timeout
from datetime import datetime

class TestInventoriesEndpoint(unittest.TestCase):
    def setUp(self):
        timeout = Timeout(55.0)  
        self.client = Client(timeout=timeout)
        self.base_url = "http://localhost:5000/api/v1/inventories/"
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json"
        }
        self.created_inventory_id = None  

        self.test_inventory = {
            "item_id": "test-item-001",
            "description": "Test inventory item",
            "item_reference": "test-ref-001",
            "locations": [1, 2, 3],
            "total_on_hand": 100,
            "total_expected": 50,
            "total_ordered": 25,
            "total_allocated": 10,
            "total_available": 65,
            "created_at": datetime.utcnow().isoformat() + "Z",
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "is_deleted": False
        }

        self.updated_inventory = {
            "item_id": "updated-item-001",
            "description": "Updated description",
            "item_reference": "updated-ref-001",
            "locations": [4, 5],
            "total_on_hand": 200,
            "total_expected": 100,
            "total_ordered": 50,
            "total_allocated": 20,
            "total_available": 130,
            "created_at": self.test_inventory["created_at"],
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "is_deleted": False
        }


    def test_2_get_inventory(self):
        if not self.created_inventory_id:
            self.skipTest("Inventory not created in test_1_post_inventory")
        response = self.client.get(f"{self.base_url}{self.created_inventory_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.created_inventory_id)

    def test_3_update_inventory(self):
        if not self.created_inventory_id:
            self.skipTest("Inventory not created in test_1_post_inventory")
        response = self.client.put(f"{self.base_url}{self.created_inventory_id}", json=self.updated_inventory)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["description"], self.updated_inventory["description"])

    def test_4_get_all_inventories(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_inventory(self):
        if not self.created_inventory_id:
            self.skipTest("Inventory not created in test_1_post_inventory")
        response = self.client.delete(f"{self.base_url}{self.created_inventory_id}")
        self.assertEqual(response.status_code, 204)

        # Check soft delete: GET geeft 404
        get_response = self.client.get(f"{self.base_url}{self.created_inventory_id}")
        self.assertEqual(get_response.status_code, 404)

    def tearDown(self):
        self.client.close()

if __name__ == "__main__":
    unittest.main()
