import unittest
from httpx import Client, Timeout
from datetime import datetime
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
            "ItemId": "test-123",
            "Description": "Integration Test Inventory",
            "ItemReference": "ref-001",
            "Locations": [1],
            "TotalOnHand": 100,
            "TotalExpected": 50,
            "TotalOrdered": 20,
            "TotalAllocated": 10,
            "TotalAvailable": 120,
            "CreatedAt": now_utc,
            "UpdatedAt": now_utc,
            "InventoryLocations": [
                {
                    "InventoryId": 0,
                    "LocationId": 1,
                    "CreatedAt": now_utc,
                    "UpdatedAt": now_utc,
                }
            ],
        }

        self.UPDATE_INVENTORY = self.TEST_INVENTORY.copy()
        self.UPDATE_INVENTORY["Description"] = "Updated Inventory"

    def test_1_create_inventory(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        print("RESPONSE TEXT:", response.text)
        self.assertIn(response.status_code, [200, 201])
        json_resp = response.json()
        print("REQUEST JSON:", json.dumps(self.TEST_INVENTORY, indent=2))
        self.TEST_INVENTORY_ID = json_resp.get("id") or json_resp.get("Id")
        self.assertIsNotNone(self.TEST_INVENTORY_ID)
        self.assertEqual(
            json_resp.get("itemId"),
            self.TEST_INVENTORY["ItemId"],
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