import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestInventoriesEndpoint(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        cls.base_url = "http://localhost:5000/api/v1/inventories/"
        timeout = Timeout(60.0)
        cls.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )

        # Create location
        now = datetime.now(timezone.utc).isoformat()
        location_response = cls.client.post(
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
        assert location_response.status_code in [
            200,
            201,
        ], f"Location creation failed: {location_response.text}"
        cls.LOCATION_ID = location_response.json().get("id")

        cls.TEST_INVENTORY = {
            "ItemId": "test-123",
            "Description": "Integration Test Inventory",
            "ItemReference": "ref-001",
            "Locations": [cls.LOCATION_ID],
            "TotalOnHand": 100,
            "TotalExpected": 50,
            "TotalOrdered": 20,
            "TotalAllocated": 10,
            "TotalAvailable": 120,
            "CreatedAt": now,
            "UpdatedAt": now,
            "InventoryLocations": [
                {
                    "InventoryId": 0,
                    "LocationId": cls.LOCATION_ID,
                    "CreatedAt": now,
                    "UpdatedAt": now,
                }
            ],
        }

        cls.UPDATE_INVENTORY = cls.TEST_INVENTORY.copy()
        cls.UPDATE_INVENTORY["TotalOnHand"] = 200
        cls.UPDATE_INVENTORY["TotalExpected"] = 100
        cls.UPDATE_INVENTORY["TotalOrdered"] = 50
        cls.UPDATE_INVENTORY["TotalAllocated"] = 25
        cls.UPDATE_INVENTORY["TotalAvailable"] = 225

    def test_0_create_invalid_inventory(self):
        invalid_payload = self.TEST_INVENTORY.copy()
        invalid_payload["ItemId"] = None
        invalid_payload["InventoryLocations"] = []  # kan niet leeg zijn

        response = self.client.post(self.base_url, json=invalid_payload)
        self.assertIn(response.status_code, [400, 422])

    def test_1_create_inventory(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        TestInventoriesEndpoint.TEST_INVENTORY_ID = data.get("id") or data.get("Id")
        self.assertIsNotNone(self.TEST_INVENTORY_ID)

    def test_2_get_inventory_by_id(self):
        response = self.client.get(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data.get("id") or data.get("Id"), self.TEST_INVENTORY_ID)

    def test_3_update_inventory(self):
        response = self.client.put(
            f"{self.base_url}{self.TEST_INVENTORY_ID}", json=self.UPDATE_INVENTORY
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(
            data.get("description") or data.get("Description"),
            self.UPDATE_INVENTORY["Description"],
        )

    def test_4_delete_inventory(self):
        response = self.client.delete(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertIn(response.status_code, [200, 204])

        response_get = self.client.get(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertIn(response_get.status_code, [404, 410])

    @classmethod
    def tearDownClass(cls):
        cls.client.close()


if __name__ == "__main__":
    unittest.main()
