import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
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

        now_iso = datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")

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
            "created_at": now_iso,
            "updated_at": now_iso,
            "isDeleted": False,
        }

        self.updated_warehouse = {
            **self.test_warehouse,
            "code": "WH002",
            "name": "Updated Warehouse",
            "city": "Updated City",
            "contact": {
                "name": "Jane Smith",
                "phone": "+0987654321",
                "email": "jane.smith@updated.com",
            },
            "updated_at": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        }

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_create_warehouse(self):
        response = self.client.post(self.base_url, json=self.test_warehouse)
        self.assertIn(
            response.status_code, [200, 201], f"Failed to create: {response.text}"
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(data["name"], self.test_warehouse["name"])
        self.assertEqual(
            data["contact"]["name"], self.test_warehouse["contact"]["name"]
        )

    def test_2_get_by_id(self):
        self.test_1_create_warehouse()
        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)

    def test_3_update_warehouse(self):
        self.test_1_create_warehouse()
        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_warehouse
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["name"], self.updated_warehouse["name"])
        self.assertEqual(
            data["contact"]["email"], self.updated_warehouse["contact"]["email"]
        )

    def test_4_get_all(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_5_soft_delete(self):
        self.test_1_create_warehouse()
        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 204)
        response_check = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertIn(response_check.status_code, [404, 410])


if __name__ == "__main__":
    unittest.main()
