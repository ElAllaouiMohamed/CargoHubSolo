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

        now = datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")
        location_response = cls.client.post(
            "http://localhost:5000/api/v1/locations/",
            json={
                "warehouseId": 1,
                "code": "INV-LOC",
                "name": "Inventory Location",
                "createdAt": now,
                "updatedAt": now,
                "isDeleted": False,
            },
        )
        assert location_response.status_code in [
            200,
            201,
        ], f"Location creation failed: {location_response.text}"
        cls.LOCATION_ID = location_response.json().get("id")

        cls.TEST_INVENTORY = {
            "itemId": "test-123",
            "description": "Integration Test Inventory",
            "itemReference": "ref-001",
            "locations": [cls.LOCATION_ID],
            "totalOnHand": 100,
            "totalExpected": 50,
            "totalOrdered": 20,
            "totalAllocated": 10,
            "totalAvailable": 120,
            "createdAt": now,
            "updatedAt": now,
            "inventoryLocations": [
                {
                    "inventoryId": 0,
                    "locationId": cls.LOCATION_ID,
                    "createdAt": now,
                    "updatedAt": now,
                }
            ],
        }

        cls.UPDATE_INVENTORY = cls.TEST_INVENTORY.copy()
        cls.UPDATE_INVENTORY["totalOnHand"] = 200
        cls.UPDATE_INVENTORY["totalExpected"] = 100
        cls.UPDATE_INVENTORY["totalOrdered"] = 50
        cls.UPDATE_INVENTORY["totalAllocated"] = 25
        cls.UPDATE_INVENTORY["totalAvailable"] = 225
        cls.UPDATE_INVENTORY["updatedAt"] = (
            datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")
        )

        cls.INVALID_INVENTORY = {
            "itemId": None,
            "description": None,
            "itemReference": "ref-invalid",
            "locations": [],
            "totalOnHand": -10,
            "totalExpected": -5,
            "totalOrdered": -2,
            "totalAllocated": -1,
            "totalAvailable": -10,
            "createdAt": now,
            "updatedAt": now,
            "inventoryLocations": [],
        }

    def test_1_create_invalid_inventory(self):
        response = self.client.post(self.base_url, json=self.INVALID_INVENTORY)
        self.assertIn(
            response.status_code,
            [400, 422],
            f"Expected validation error: {response.text}",
        )
        data = response.json()
        self.assertIn("errors", data, "Expected validation errors in response")
        errors = str(data["errors"].values())
        self.assertTrue(
            any(
                [
                    "ItemId is verplicht" in errors,
                    "Description is verplicht" in errors,
                    "InventoryLocations is verplicht" in errors,
                    "TotalOnHand moet ≥ 0 zijn" in errors,
                    "TotalExpected moet ≥ 0 zijn" in errors,
                    "TotalOrdered moet ≥ 0 zijn" in errors,
                    "TotalAllocated moet ≥ 0 zijn" in errors,
                    "TotalAvailable moet ≥ 0 zijn" in errors,
                ]
            ),
            "Expected specific validation error messages",
        )

    def test_2_create_inventory(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        self.assertIn(
            response.status_code, [200, 201], f"Creation failed: {response.text}"
        )
        data = response.json()
        TestInventoriesEndpoint.TEST_INVENTORY_ID = data.get("id") or data.get("Id")
        self.assertIsNotNone(self.TEST_INVENTORY_ID)
        self.assertEqual(data.get("description"), self.TEST_INVENTORY["description"])

    def test_3_get_inventory_by_id(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        TestInventoriesEndpoint.TEST_INVENTORY_ID = data.get("id") or data.get("Id")

        response = self.client.get(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data.get("id") or data.get("Id"), self.TEST_INVENTORY_ID)

    def test_4_update_inventory(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        TestInventoriesEndpoint.TEST_INVENTORY_ID = data.get("id") or data.get("Id")

        response = self.client.put(
            f"{self.base_url}{self.TEST_INVENTORY_ID}", json=self.UPDATE_INVENTORY
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data.get("totalOnHand"), self.UPDATE_INVENTORY["totalOnHand"])
        self.assertEqual(
            data.get("totalExpected"), self.UPDATE_INVENTORY["totalExpected"]
        )
        self.assertEqual(
            data.get("totalOrdered"), self.UPDATE_INVENTORY["totalOrdered"]
        )
        self.assertEqual(
            data.get("totalAllocated"), self.UPDATE_INVENTORY["totalAllocated"]
        )
        self.assertEqual(
            data.get("totalAvailable"), self.UPDATE_INVENTORY["totalAvailable"]
        )

    def test_5_delete_inventory(self):
        response = self.client.post(self.base_url, json=self.TEST_INVENTORY)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        TestInventoriesEndpoint.TEST_INVENTORY_ID = data.get("id") or data.get("Id")

        response = self.client.delete(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertIn(response.status_code, [200, 204])

        response_get = self.client.get(f"{self.base_url}{self.TEST_INVENTORY_ID}")
        self.assertIn(response_get.status_code, [404, 410])

    @classmethod
    def tearDownClass(cls):
        if hasattr(cls, "LOCATION_ID"):
            cls.client.delete(
                f"http://localhost:5000/api/v1/locations/{cls.LOCATION_ID}"
            )
        if hasattr(cls, "TEST_INVENTORY_ID"):
            cls.client.delete(f"{cls.base_url}{cls.TEST_INVENTORY_ID}")
        cls.client.close()


if __name__ == "__main__":
    unittest.main()
