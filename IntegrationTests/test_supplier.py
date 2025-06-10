import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestSuppliersEndpoint(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.base_url = "http://localhost:5000/api/v1/suppliers/"
        api_key = os.getenv("TEST_API_KEY", "fallback")
        cls.client = Client(
            timeout=Timeout(60.0),
            headers={"X-Api-Key": api_key, "Content-Type": "application/json"},
        )
        now = datetime.now(timezone.utc).isoformat()

        cls.test_supplier = {
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

        cls.updated_supplier = {
            **cls.test_supplier,
            "name": "Updated Supplier",
            "code": "SUPTEST002",
            "reference": "REFTEST002",
            "updated_at": datetime.now(timezone.utc).isoformat(),
        }

        cls.created_id = None

    def test_1_create_supplier(self):
        res = self.client.post(self.base_url, json=self.test_supplier)
        self.assertIn(res.status_code, [200, 201], f"Create failed: {res.text}")
        data = res.json()
        TestSuppliersEndpoint.created_id = data.get("id")
        self.assertIsNotNone(self.created_id)
        self.assertEqual(data["name"], self.test_supplier["name"])
        self.assertEqual(data["code"], self.test_supplier["code"])
        self.assertFalse(data["isDeleted"])

    def test_2_get_supplier_by_id(self):
        if not self.created_id:
            self.skipTest("Create failed")
        res = self.client.get(f"{self.base_url}{self.created_id}")
        self.assertEqual(res.status_code, 200)
        self.assertEqual(res.json()["id"], self.created_id)

    def test_3_update_supplier(self):
        if not self.created_id:
            self.skipTest("Create failed")
        res = self.client.put(
            f"{self.base_url}{self.created_id}", json=self.updated_supplier
        )
        self.assertEqual(res.status_code, 200)
        data = res.json()
        self.assertEqual(data["name"], self.updated_supplier["name"])
        self.assertEqual(data["reference"], self.updated_supplier["reference"])

    def test_4_get_all_suppliers(self):
        res = self.client.get(self.base_url)
        self.assertEqual(res.status_code, 200)
        self.assertIsInstance(res.json(), list)

    def test_5_soft_delete_supplier(self):
        if not self.created_id:
            self.skipTest("Create failed")
        res = self.client.delete(f"{self.base_url}{self.created_id}")
        self.assertIn(res.status_code, [200, 204])
        res_check = self.client.get(f"{self.base_url}{self.created_id}")
        self.assertIn(res_check.status_code, [404, 410])

    @classmethod
    def tearDownClass(cls):
        if cls.created_id:
            cls.client.delete(f"{cls.base_url}{cls.created_id}")
        cls.client.close()


if __name__ == "__main__":
    unittest.main()
