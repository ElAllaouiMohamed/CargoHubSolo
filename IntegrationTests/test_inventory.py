import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os
import json

class TestInventoriesEndpoint(unittest.TestCase):
    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/inventories/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.TEST_INVENTORY_ID = None

        now_utc = datetime.utcnow().isoformat() + "Z"

        self.TEST_INVENTORY = {
            "item_id": "test-123",
            "description": "Integration Test Inventory",
            "item_reference": "ref-001",
            "locations": [1],
            "total_on_hand": 100,
            "total_expected": 50,
            "total_ordered": 20,
            "total_allocated": 10,
            "total_available": 120,
            "created_at": now_utc,
            "updated_at": now_utc,
        }

        self.UPDATE_INVENTORY = {
            "item_id": "test-123",
            "description": "Integration Test Inventory",
            "item_reference": "ref-001",
            "locations": [1],
            "total_on_hand": 100,
            "total_expected": 50,
            "total_ordered": 20,
            "total_allocated": 10,
            "total_available": 120,
            "created_at": now_utc,
            "updated_at": now_utc,
        }
    def test_1_create_inventory(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        print("RESPONSE TEXT:", response.text)
        self.assertIn(response.status_code, [200, 201])
        json_resp = response.json()
        print("REQUEST JSON:", json.dumps(self.TEST_INVENTORY, indent=2))
        self.TEST_INVENTORY_ID = json_resp.get("id") or json_resp.get("Id")
        self.assertIsNotNone(self.TEST_INVENTORY_ID)
        self.assertEqual(
            json_resp.get("item_id") or json_resp.get("ItemId"),
            self.TEST_INVENTORY["item_id"],
        )

    def test_2_get_inventory_by_id(self):
        if not self.TEST_INVENTORY_ID:
            self.skipTest("Create inventory test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(
            json_resp.get("id") or json_resp.get("Id"), self.TEST_INVENTORY_ID
        )

    def test_3_update_inventory(self):
        if not self.TEST_INVENTORY_ID:
            self.skipTest("Create inventory test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.TEST_INVENTORY_ID}", json=self.UPDATE_INVENTORY
        )
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(
            json_resp.get("Description") or json_resp.get("description"),
            self.UPDATE_INVENTORY["Description"],
        )

    def test_4_delete_inventory(self):
        if not self.TEST_INVENTORY_ID:
            self.skipTest("Create inventory test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertIn(response.status_code, [200, 204])

        response_get = self.client.get(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertIn(response_get.status_code, [404, 410])

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
