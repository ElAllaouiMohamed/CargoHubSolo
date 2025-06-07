import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout
import os


class TestItemsEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/items/"
        timeout = Timeout(60.0)
        self.client = Client(timeout=timeout)
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.client.headers = {
            "X-Api-Key": api_key,
            "Content-Type": "application/json",
        }
        self.created_item_id = None

        self.test_item = {
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
            "created_at": datetime.utcnow().isoformat() + "Z",
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "is_deleted": False,
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
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "is_deleted": False,
        }

    def test_2_get_item(self):
        if not self.created_item_id:
            self.skipTest("Create item failed or did not run")

        response = self.client.get(f"{self.base_url}{self.created_item_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data.get("id") or data.get("Id"), self.created_item_id)

    def test_3_update_item(self):
        if not self.created_item_id:
            self.skipTest("Create item failed or did not run")

        response = self.client.put(
            f"{self.base_url}{self.created_item_id}", json=self.updated_item
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(
            data.get("description") or data.get("Description"),
            self.updated_item["description"],
        )

    def test_4_get_all_items(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_item(self):
        if not self.created_item_id:
            self.skipTest("Create item failed or did not run")

        response = self.client.delete(f"{self.base_url}{self.created_item_id}")
        self.assertIn(response.status_code, (200, 204))

        # Controleer dat item niet meer gevonden wordt (soft delete)
        get_response = self.client.get(f"{self.base_url}{self.created_item_id}")
        self.assertIn(get_response.status_code, (404, 410))

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
