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

        now = datetime.utcnow().isoformat() + "Z"
        location_response = self.client.post(
            "http://localhost:5000/api/v1/locations/",
            json={
                "WarehouseId": 1,
                "Code": "INV-LOC",
                "Name": "Inventory Location",
                "CreatedAt": now,
                "UpdatedAt": now,
                "IsDeleted": False,
            },
        )
        assert location_response.status_code in [200, 201]
        self.LOCATION_ID = location_response.json().get("id")

        now_utc = datetime.utcnow().isoformat() + "Z"

        self.TEST_INVENTORY = {
            "ItemId": "test-123",
            "Description": "Integration Test Inventory",
            "ItemReference": "ref-001",
            "Locations": [self.LOCATION_ID],
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
                    "LocationId": self.LOCATION_ID,
                    "CreatedAt": now_utc,
                    "UpdatedAt": now_utc,
                }
            ],
        }

        self.UPDATE_INVENTORY = {
            "ItemId": "test-123",
            "Description": "Integration Test Inventory",
            "ItemReference": "ref-001",
            "Locations": [self.LOCATION_ID],
            "TotalOnHand": 200,
            "TotalExpected": 100,
            "TotalOrdered": 50,
            "TotalAllocated": 25,
            "TotalAvailable": 225,
            "CreatedAt": now_utc,
            "UpdatedAt": now_utc,
            "InventoryLocations": [
                {
                    "InventoryId": 0,
                    "LocationId": self.LOCATION_ID,
                    "CreatedAt": now_utc,
                    "UpdatedAt": now_utc,
                }
            ],
        }

    def test_1_create_inventory(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        print("RESPONSE TEXT:", response.text)
        print("REQUEST JSON:", json.dumps(self.TEST_INVENTORY, indent=2))
        self.assertIn(response.status_code, [200, 201])
        json_resp = response.json()
        self.TEST_INVENTORY_ID = json_resp.get("id") or json_resp.get("Id")
        self.assertIsNotNone(self.TEST_INVENTORY_ID)
        self.assertEqual(
            json_resp.get("itemId") or json_resp.get("ItemId"),
            self.TEST_INVENTORY["ItemId"]
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
