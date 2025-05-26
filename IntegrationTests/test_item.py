import unittest
from httpx import Client
from datetime import datetime

class TestItemsEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/items/"
        self.client = Client()
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json"
        }
        self.test_id = 9999

        self.test_item = {
            "id": self.test_id,
            "uid": "test-uid",
            "code": "test-code",
            "description": "Test item description",
            "short_description": "Test short desc",
            "upc_code": "123456789012",
            "model_number": "MN-001",
            "commodity_code": "CC-001",
            "item_line": None,
            "item_group": None,
            "item_type": None,
            "unit_purchase_quantity": 10,
            "unit_order_quantity": 5,
            "pack_order_quantity": 2,
            "supplier_id": 1,
            "supplier_code": "SUP001",
            "supplier_part_number": "SPN-001",
            "weight_in_kg": 100,
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "is_deleted": False
        }

        self.updated_item = {
            "uid": "updated-uid",
            "code": "updated-code",
            "description": "Updated description",
            "short_description": "Updated short desc",
            "upc_code": "987654321098",
            "model_number": "MN-002",
            "commodity_code": "CC-002",
            "item_line": None,
            "item_group": None,
            "item_type": None,
            "unit_purchase_quantity": 20,
            "unit_order_quantity": 10,
            "pack_order_quantity": 5,
            "supplier_id": 1,
            "supplier_code": "SUP001",
            "supplier_part_number": "SPN-002",
            "weight_in_kg": 200,
            "created_at": self.test_item["created_at"],
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "is_deleted": False
        }

    def test_1_post_item(self):
        response = self.client.post(self.base_url, json=self.test_item)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.assertEqual(data["uid"], self.test_item["uid"])

    def test_2_get_item(self):
        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)

    def test_3_update_item(self):
        response = self.client.put(f"{self.base_url}{self.test_id}", json=self.updated_item)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["description"], self.updated_item["description"])

    def test_4_get_all_items(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_item(self):
        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 204)  # NoContent

        # Verify item is not accessible after soft delete
        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(get_response.status_code, 404)

if __name__ == "__main__":
    unittest.main()
