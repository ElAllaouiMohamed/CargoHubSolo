import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os
import json


class TestItemsEndpoint(unittest.TestCase):

    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/items/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.created_item_id = None

        self.test_item = {
            "UId": "test-uid",
            "Code": "test-code",
            "Description": "Test item description",
            "ShortDescription": "Test short desc",
            "UpcCode": "123456789012",
            "ModelNumber": "MN-001",
            "CommodityCode": "CC-001",
            "ItemLineId": 1,
            "ItemGroupId": 1,
            "ItemTypeId": 1,
            "UnitPurchaseQuantity": 0,
            "UnitOrderQuantity": 0,
            "PackOrderQuantity": 0,
            "SupplierId": 1,
            "SupplierCode": "SUP001",
            "SupplierPartNumber": "SPN-001",
            "WeightInKg": 100,
            "IsDeleted": False,
        }

        self.updated_item = {
            "UId": "updated-uid",
            "Code": "updated-code",
            "Description": "Updated description",
            "ShortDescription": "Updated short desc",
            "UpcCode": "987654321098",
            "ModelNumber": "MN-002",
            "CommodityCode": "CC-002",
            "ItemLineId": 1,
            "ItemGroupId": 1,
            "ItemTypeId": 1,
            "UnitPurchaseQuantity": 20,
            "UnitOrderQuantity": 10,
            "PackOrderQuantity": 5,
            "SupplierId": 1,
            "SupplierCode": "SUP001",
            "SupplierPartNumber": "SPN-002",
            "WeightInKg": 200,
            "IsDeleted": False,
        }

    def test_1_create_item(self):
        response = self.client.post(self.base_url, json=self.test_item)
        print("RESPONSE TEXT:", response.text)
        print("REQUEST JSON:", json.dumps(self.test_item, indent=2))
        self.assertIn(response.status_code, [200, 201])
        json_resp = response.json()
        self.created_item_id = json_resp.get("id") or json_resp.get("Id")
        self.assertIsNotNone(self.created_item_id)
        self.assertEqual(
            json_resp.get("Description") or json_resp.get("description"),
            self.test_item["Description"],
        )

    def test_2_get_item(self):
        if not self.created_item_id:
            self.skipTest("Create item test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.created_item_id}")
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(
            json_resp.get("id") or json_resp.get("Id"), self.created_item_id
        )

    def test_3_update_item(self):
        if not self.created_item_id:
            self.skipTest("Create item test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.created_item_id}", json=self.updated_item
        )
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(
            json_resp.get("Description") or json_resp.get("description"),
            self.updated_item["Description"],
        )

    def test_4_get_all_items(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_item(self):
        if not self.created_item_id:
            self.skipTest("Create item test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.created_item_id}")
        self.assertIn(response.status_code, [200, 204])

        # Controleer dat item niet meer gevonden wordt (soft delete)
        get_response = self.client.get(f"{self.base_url}{self.created_item_id}")
        self.assertIn(get_response.status_code, [404, 410])

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
