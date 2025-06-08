import unittest
from httpx import Client, Timeout
from datetime import datetime
import os


class TestSuppliersEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/suppliers/"
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.client = Client(
            timeout=Timeout(60.0),
            headers={"X-Api-Key": api_key, "Content-Type": "application/json"},
        )
        self.test_id = None

        now = datetime.utcnow().isoformat() + "Z"
        self.test_supplier = {
            "code": "SUPTEST001",
            "name": "Test Supplier",
            "address": "123 Supplier St",
            "addressExtra": "Suite A",
            "city": "Test City",
            "zipCode": "10001",
            "province": "Test State",
            "country": "Testland",
            "contactName": "Jane Doe",
            "phoneNumber": "+1234567890",
            "reference": "REFTEST001",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

        self.updated_supplier = {
            **self.test_supplier,
            "name": "Updated Supplier",
            "code": "SUPTEST002",
            "reference": "REFTEST002",
        }

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_create_supplier(self):
        res = self.client.post(self.base_url, json=self.test_supplier)
        self.assertIn(res.status_code, [200, 201], f"Create failed: {res.text}")
        data = res.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(data["name"], self.test_supplier["name"])
        self.assertEqual(data["code"], self.test_supplier["code"])
        self.assertFalse(data["isDeleted"])

    def test_2_get_supplier_by_id(self):
        self.test_1_create_supplier()
        res = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(res.status_code, 200)
        data = res.json()
        self.assertEqual(data["id"], self.test_id)

    def test_3_update_supplier(self):
        self.test_1_create_supplier()
        res = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_supplier
        )
        self.assertEqual(res.status_code, 200, f"Update failed: {res.text}")
        data = res.json()
        self.assertEqual(data["name"], self.updated_supplier["name"])
        self.assertEqual(data["reference"], self.updated_supplier["reference"])

    def test_4_get_all_suppliers(self):
        self.test_1_create_supplier()
        res = self.client.get(self.base_url)
        self.assertEqual(res.status_code, 200)
        data = res.json()
        self.assertTrue(any(s["id"] == self.test_id for s in data))

    def test_5_soft_delete_supplier(self):
        self.test_1_create_supplier()
        res = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertIn(res.status_code, [204, 200])
        follow_up = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(follow_up.status_code, 404)


if __name__ == "__main__":
    unittest.main()
