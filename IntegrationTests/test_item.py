import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestItemsEndpoint(unittest.TestCase):

    @classmethod
    def setUpClass(cls):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        timeout = Timeout(60.0)
        cls.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        cls.base_url = "http://localhost:5000/api/v1/items/"
        now = datetime.now(timezone.utc).isoformat()

        cls.supplier_id = cls.create_supplier()
        cls.item_line_id = cls.create_item_line()
        cls.item_group_id = cls.create_item_group()
        cls.item_type_id = cls.create_item_type()

        cls.test_item = {
            "UId": "test-uid",
            "Code": "test-code",
            "Description": "Test item description",
            "ShortDescription": "Test short desc",
            "UpcCode": "123456789012",
            "ModelNumber": "MN-001",
            "CommodityCode": "CC-001",
            "ItemLineId": cls.item_line_id,
            "ItemGroupId": cls.item_group_id,
            "ItemTypeId": cls.item_type_id,
            "UnitPurchaseQuantity": 10,
            "UnitOrderQuantity": 5,
            "PackOrderQuantity": 2,
            "SupplierId": cls.supplier_id,
            "SupplierCode": "SUP001",
            "SupplierPartNumber": "SPN-001",
            "WeightInKg": 100,
            "CreatedAt": now,
            "UpdatedAt": now,
            "IsDeleted": False,
        }

        cls.updated_item = {
            **cls.test_item,
            "Description": "Updated description",
            "Code": "updated-code",
            "SupplierPartNumber": "SPN-002",
            "WeightInKg": 200,
            "UpdatedAt": datetime.now(timezone.utc).isoformat(),
        }

    @classmethod
    def create_supplier(cls):
        now = datetime.now(timezone.utc).isoformat()
        payload = {
            "code": "SUPITEM001",
            "name": "Item Supplier",
            "address": "Straat 12",
            "addressExtra": "Unit A",
            "city": "Teststad",
            "zipCode": "1234AB",
            "province": "Testprovincie",
            "country": "Nederland",
            "contactName": "Test Persoon",
            "phoneNumber": "0612345678",
            "reference": "ITEM-REF-01",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }
        response = cls.client.post(
            "http://localhost:5000/api/v1/suppliers/", json=payload
        )
        assert response.status_code in [
            200,
            201,
        ], f"Create supplier failed: {response.text}"
        return response.json()["id"]

    @classmethod
    def create_item_line(cls):
        now = datetime.now(timezone.utc).isoformat()
        response = cls.client.post(
            "http://localhost:5000/api/v1/itemlines/",
            json={
                "Name": "Test Line",
                "Description": "Line desc",
                "CreatedAt": now,
                "UpdatedAt": now,
            },
        )
        assert response.status_code in [
            200,
            201,
        ], f"Create itemline failed: {response.text}"
        return response.json()["id"]

    @classmethod
    def create_item_group(cls):
        now = datetime.now(timezone.utc).isoformat()
        response = cls.client.post(
            "http://localhost:5000/api/v1/itemgroups/",
            json={
                "Name": "Test Group",
                "Description": "Group desc",
                "CreatedAt": now,
                "UpdatedAt": now,
            },
        )
        assert response.status_code in [
            200,
            201,
        ], f"Create itemgroup failed: {response.text}"
        return response.json()["id"]

    @classmethod
    def create_item_type(cls):
        now = datetime.now(timezone.utc).isoformat()
        response = cls.client.post(
            "http://localhost:5000/api/v1/itemtypes/",
            json={
                "Name": "Test Type",
                "Description": "Type desc",
                "CreatedAt": now,
                "UpdatedAt": now,
            },
        )
        assert response.status_code in [
            200,
            201,
        ], f"Create itemtype failed: {response.text}"
        return response.json()["id"]

    def test_1_create_item(self):
        response = self.client.post(self.base_url, json=self.test_item)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.__class__.created_item_id = data.get("id") or data.get("Id")
        self.assertIsNotNone(self.created_item_id)

    def test_2_get_item(self):
        self.test_1_create_item()
        response = self.client.get(f"{self.base_url}{self.created_item_id}")
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["id"], self.created_item_id)

    def test_3_update_item(self):
        self.test_1_create_item()
        response = self.client.put(
            f"{self.base_url}{self.created_item_id}", json=self.updated_item
        )
        self.assertEqual(response.status_code, 200)
        self.assertIn("Updated description", response.text)

    def test_4_get_all_items(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_5_soft_delete_item(self):
        self.test_1_create_item()
        response = self.client.delete(f"{self.base_url}{self.created_item_id}")
        self.assertIn(response.status_code, [200, 204])
        response = self.client.get(f"{self.base_url}{self.created_item_id}")
        self.assertIn(response.status_code, [404, 410])

    @classmethod
    def tearDownClass(cls):
        cls.client.close()


if __name__ == "__main__":
    unittest.main()
