import unittest
from httpx import Client
from datetime import datetime

class TestOrdersEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/orders/"
        self.client = Client()
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json"
        }
        self.test_id = None

        self.test_order = {
            "sourceId": 1,
            "orderDate": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "requestDate": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "reference": "TEST-ORDER-001",
            "reference_extra": "EXTRA-001",
            "order_status": "Pending",
            "notes": "Test order notes",
            "shippingNotes": "Test shipping notes",
            "pickingNotes": "Test picking notes",
            "warehouseId": 1,
            "shipTo": "123 Test St",
            "billTo": "456 Test Ave",
            "shipmentId": 1,
            "totalAmount": 100.0,
            "totalDiscount": 10.0,
            "totalTax": 5.0,
            "totalSurcharge": 2.0,
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False,
            "items": [
                {
                    "itemId": 1,
                    "quantity": 5,
                    "unitPrice": 10.0
                }
            ]
        }

        self.updated_order = {
            "sourceId": 2,
            "orderDate": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "requestDate": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "reference": "UPDATED-ORDER-001",
            "reference_extra": "UPDATED-EXTRA-001",
            "order_status": "Shipped",
            "notes": "Updated order notes",
            "shippingNotes": "Updated shipping notes",
            "pickingNotes": "Updated picking notes",
            "warehouseId": 2,
            "shipTo": "789 Updated St",
            "billTo": "012 Updated Ave",
            "shipmentId": 2,
            "totalAmount": 200.0,
            "totalDiscount": 20.0,
            "totalTax": 10.0,
            "totalSurcharge": 4.0,
            "created_at": self.test_order["created_at"],
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "isDeleted": False,
            "items": [
                {
                    "itemId": 2,
                    "quantity": 10,
                    "unitPrice": 15.0
                }
            ]
        }

        response = self.client.get(self.base_url)
        if response.status_code == 200:
            orders = response.json()
            for order in orders:
                if order.get("reference") in ["TEST-ORDER-001", "UPDATED-ORDER-001"]:
                    self.client.delete(f"{self.base_url}{order['id']}")

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_post_order(self):
        response = self.client.post(self.base_url, json=self.test_order)
        self.assertIn(response.status_code, (200, 201), f"Failed to create order: {response.text}")
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id, "Order ID should be returned")
        self.assertEqual(data["reference"], self.test_order["reference"])
        self.assertEqual(data["totalAmount"], self.test_order["totalAmount"])
        self.assertFalse(data["isDeleted"])
        self.assertEqual(len(data["items"]), 1)
        self.assertEqual(data["items"][0]["quantity"], 5)

    def test_2_get_order(self):
        if not self.test_id:
            self.skipTest("Create order test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200, f"Failed to get order: {response.text}")
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["reference"], self.test_order["reference"])
        self.assertEqual(data["totalAmount"], self.test_order["totalAmount"])

    def test_3_update_order(self):
        if not self.test_id:
            self.skipTest("Create order test failed or not run.")

        response = self.client.put(f"{self.base_url}{self.test_id}", json=self.updated_order)
        self.assertEqual(response.status_code, 200, f"Failed to update order: {response.text}")
        data = response.json()
        self.assertEqual(data["reference"], self.updated_order["reference"])
        self.assertEqual(data["totalAmount"], self.updated_order["totalAmount"])
        self.assertFalse(data["isDeleted"])
        self.assertEqual(len(data["items"]), 1)
        self.assertEqual(data["items"][0]["quantity"], 10)

    def test_4_get_all_orders(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200, f"Failed to get all orders: {response.text}")
        data = response.json()
        self.assertTrue(isinstance(data, list))
        if self.test_id:
            found = any(order["id"] == self.test_id for order in data)
            self.assertTrue(found, "Created order should be in the list")

    def test_5_soft_delete_order(self):
        if not self.test_id:
            self.skipTest("Create order test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 204, f"Failed to soft delete order: {response.text}")

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(get_response.status_code, 404, "Soft-deleted order should return 404")

if __name__ == "__main__":
    unittest.main()