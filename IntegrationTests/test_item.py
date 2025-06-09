import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os
import json


class TestItemsEndpoint(unittest.TestCase):

    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.base_url = "http://localhost:5000/api/v1/items/"
        self.created_item_id = None

        now = datetime.now(timezone.utc).isoformat()

        # Create dependent foreign keys
        self.supplier_id = self.create_supplier()
        self.item_line_id = self.create_item_line()
        self.item_group_id = self.create_item_group()
        self.item_type_id = self.create_item_type()

        self.test_item = {
            "UId": "test-uid",
            "Code": "test-code",
            "Description": "Test item description",
            "ShortDescription": "Test short desc",
            "UpcCode": "123456789012",
            "ModelNumber": "MN-001",
            "CommodityCode": "CC-001",
            "ItemLineId": self.item_line_id,
            "ItemGroupId": self.item_group_id,
            "ItemTypeId": self.item_type_id,
            "UnitPurchaseQuantity": 10,
            "UnitOrderQuantity": 5,
            "PackOrderQuantity": 2,
            "SupplierId": self.supplier_id,
            "SupplierCode": "SUP001",
            "SupplierPartNumber": "SPN-001",
            "WeightInKg": 100,
            "CreatedAt": now,
            "UpdatedAt": now,
            "IsDeleted": False,
        }

        self.updated_item = {
            **self.test_item,
            "Description": "Updated description",
            "Code": "updated-code",
            "SupplierPartNumber": "SPN-002",
            "WeightInKg": 200,
            "UpdatedAt": datetime.now(timezone.utc).isoformat(),
        }


    def create_supplier(self):
        now = datetime.utcnow().isoformat() + "Z"
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

        response = self.client.post("http://localhost:5000/api/v1/suppliers/", json=payload)
        assert response.status_code in [
            200,
            201,
        ], f"Create supplier failed: {response.text}"
        return response.json()["id"]

    def create_item_line(self):
        now = datetime.now(timezone.utc).isoformat()
        response = self.client.post(
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

    def create_item_group(self):
        now = datetime.now(timezone.utc).isoformat()
        response = self.client.post(
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

    def create_item_type(self):
        now = datetime.now(timezone.utc).isoformat()
        response = self.client.post(
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
        print("RESPONSE TEXT:", response.text)
        print("REQUEST JSON:", json.dumps(self.test_item, indent=2))
        self.assertIn(response.status_code, [200, 201])
        json_resp = response.json()
        self.created_item_id = json_resp.get("id") or json_resp.get("Id")
        self.assertIsNotNone(self.created_item_id)

    def test_2_get_item(self):
        self.test_1_create_item()
        response = self.client.get(f"{self.base_url}{self.created_item_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.created_item_id)

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

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
