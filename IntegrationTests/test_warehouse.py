import unittest
from httpx import Client
from datetime import datetime

import os

class TestWarehousesEndpoint(unittest.TestCase):

    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/warehouses/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.test_id = None

        self.test_warehouse = {
            "code": "WH001",
            "name": "Test Warehouse",
            "address": "123 Warehouse Rd",
            "zip": "12345",
            "city": "Test City",
            "province": "Test Province",
            "country": "Test Country",
            "contact": {
                "name": "John Doe",
                "phone": "+1234567890",
                "email": "john.doe@test.com",
            },
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False,
        }

        self.updated_warehouse = {
            "code": "WH002",
            "name": "Updated Warehouse",
            "address": "456 Updated Blvd",
            "zip": "54321",
            "city": "Updated City",
            "province": "Updated Province",
            "country": "Updated Country",
            "contact": {
                "name": "Jane Smith",
                "phone": "+0987654321",
                "email": "jane.smith@updated.com",
            },
            "created_at": self.test_warehouse["created_at"],
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False,
        }

        response = self.client.get(self.base_url)
        if response.status_code == 200:
            warehouses = response.json()
            for warehouse in warehouses:
                if warehouse.get("name") in ["Test Warehouse", "Updated Warehouse"]:
                    self.client.delete(f"{self.base_url}{warehouse['id']}")

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_post_warehouse(self):
        response = self.client.post(self.base_url, json=self.test_warehouse)
        self.assertIn(
            response.status_code,
            (200, 201),
            f"Failed to create warehouse: {response.text}",
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id, "Warehouse ID should be returned")
        self.assertEqual(data["name"], self.test_warehouse["name"])
        self.assertEqual(data["code"], self.test_warehouse["code"])
        self.assertEqual(
            data["contact"]["name"], self.test_warehouse["contact"]["name"]
        )
        self.assertFalse(data["isDeleted"])

    def test_2_get_warehouse(self):
        if not self.test_id:
            self.skipTest("Create warehouse test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(
            response.status_code, 200, f"Failed to get warehouse: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["name"], self.test_warehouse["name"])
        self.assertEqual(data["code"], self.test_warehouse["code"])
        self.assertEqual(
            data["contact"]["name"], self.test_warehouse["contact"]["name"]
        )

    def test_3_update_warehouse(self):
        if not self.test_id:
            self.skipTest("Create warehouse test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_warehouse
        )
        self.assertEqual(
            response.status_code, 200, f"Failed to update warehouse: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["name"], self.updated_warehouse["name"])
        self.assertEqual(data["code"], self.updated_warehouse["code"])
        self.assertEqual(
            data["contact"]["name"], self.updated_warehouse["contact"]["name"]
        )
        self.assertFalse(data["isDeleted"])

    def test_4_get_all_warehouses(self):
        response = self.client.get(self.base_url)
        self.assertEqual(
            response.status_code, 200, f"Failed to get all warehouses: {response.text}"
        )
        data = response.json()
        self.assertTrue(isinstance(data, list))
        if self.test_id:
            found = any(warehouse["id"] == self.test_id for warehouse in data)
            self.assertTrue(found, "Created warehouse should be in the list")

    def test_5_soft_delete_warehouse(self):
        if not self.test_id:
            self.skipTest("Create warehouse test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(
            response.status_code,
            204,
            f"Failed to soft delete warehouse: {response.text}",
        )

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(
            get_response.status_code, 404, "Soft-deleted warehouse should return 404"
        )


if __name__ == "__main__":
    unittest.main()
