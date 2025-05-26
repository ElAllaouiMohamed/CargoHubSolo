import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout


class TestSuppliersEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/suppliers/"
        timeout = Timeout(60.0)
        self.client = Client(timeout=timeout)
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json",
        }
        self.test_id = None

        now_iso = datetime.utcnow().isoformat() + "Z"
        self.test_supplier = {
            "code": "SUP001",
            "name": "Test Supplier",
            "address": "123 Supplier St",
            "addressExtra": "Suite 100",
            "city": "Test City",
            "zipCode": "12345",
            "province": "Test Province",
            "country": "Test Country",
            "contactName": "John Doe",
            "phoneNumber": "+1234567890",
            "reference": "REF001",
            "created_at": now_iso,
            "updated_at": now_iso,
            "isDeleted": False,
        }

        self.updated_supplier = {
            "code": "SUP002",
            "name": "Updated Supplier",
            "address": "456 Updated Ave",
            "addressExtra": "Building B",
            "city": "Updated City",
            "zipCode": "54321",
            "province": "Updated Province",
            "country": "Updated Country",
            "contactName": "Jane Smith",
            "phoneNumber": "+0987654321",
            "reference": "REF002",
            "created_at": self.test_supplier["created_at"],
            "updated_at": now_iso,
            "isDeleted": False,
        }

        # Cleanup oude test data
        response = self.client.get(self.base_url)
        if response.status_code == 200:
            suppliers = response.json()
            for supplier in suppliers:
                if supplier.get("name") in [
                    self.test_supplier["name"],
                    self.updated_supplier["name"],
                ]:
                    self.client.delete(f"{self.base_url}{supplier['id']}")

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_post_supplier(self):
        response = self.client.post(self.base_url, json=self.test_supplier)
        self.assertIn(
            response.status_code,
            (200, 201),
            f"Failed to create supplier: {response.text}",
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id, "Supplier ID should be returned")
        self.assertEqual(data.get("name"), self.test_supplier["name"])
        self.assertEqual(data.get("code"), self.test_supplier["code"])
        self.assertFalse(data.get("isDeleted"))

    def test_2_get_supplier(self):
        if not self.test_id:
            self.skipTest("Create supplier test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(
            response.status_code, 200, f"Failed to get supplier: {response.text}"
        )
        data = response.json()
        self.assertEqual(data.get("id"), self.test_id)
        self.assertEqual(data.get("name"), self.test_supplier["name"])
        self.assertEqual(data.get("code"), self.test_supplier["code"])

    def test_3_update_supplier(self):
        if not self.test_id:
            self.skipTest("Create supplier test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_supplier
        )
        self.assertEqual(
            response.status_code, 200, f"Failed to update supplier: {response.text}"
        )
        data = response.json()
        self.assertEqual(data.get("name"), self.updated_supplier["name"])
        self.assertEqual(data.get("code"), self.updated_supplier["code"])
        self.assertFalse(data.get("isDeleted"))

    def test_4_get_all_suppliers(self):
        response = self.client.get(self.base_url)
        self.assertEqual(
            response.status_code, 200, f"Failed to get all suppliers: {response.text}"
        )
        data = response.json()
        self.assertTrue(isinstance(data, list))
        if self.test_id:
            found = any(supplier.get("id") == self.test_id for supplier in data)
            self.assertTrue(found, "Created supplier should be in the list")

    def test_5_soft_delete_supplier(self):
        if not self.test_id:
            self.skipTest("Create supplier test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertIn(
            response.status_code,
            (200, 204),
            f"Failed to soft delete supplier: {response.text}",
        )

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertIn(
            get_response.status_code,
            (404, 410),
            "Soft-deleted supplier should return 404 or 410",
        )


if __name__ == "__main__":
    unittest.main()
