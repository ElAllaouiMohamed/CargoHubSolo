import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout
import os


class TestOrdersEndpoint(unittest.TestCase):

    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/orders/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.order_id = None

        now = datetime.utcnow().isoformat() + "Z"

        self.order_payload = {
            "source_id": 1,
            "order_date": now,
            "request_date": now,
            "reference": "TEST-ORDER-001",
            "reference_extra": "EXTRA-001",
            "order_status": "Pending",
            "notes": "Test order notes",
            "shipping_notes": "Test shipping notes",
            "picking_notes": "Test picking notes",
            "warehouse_id": 1,
            "ship_to": "123 Test St",
            "bill_to": "456 Test Ave",
            "shipment_id": 1,
            "total_amount": 100,
            "total_discount": 10,
            "total_tax": 5,
            "total_surcharge": 2,
            "created_at": now,
            "updated_at": now,
            "stocks": [{"item_id": "1", "quantity": 10}],
        }

        self.updated_order = {
            "source_id": 2,
            "order_date": now,
            "request_date": now,
            "reference": "UPDATED-ORDER-001",
            "reference_extra": "UPDATED-EXTRA-001",
            "order_status": "Shipped",
            "notes": "Updated order notes",
            "shipping_notes": "Updated shipping notes",
            "picking_notes": "Updated picking notes",
            "warehouse_id": 2,
            "ship_to": "789 Updated St",
            "bill_to": "012 Updated Ave",
            "shipment_id": 2,
            "total_amount": 200,
            "total_discount": 20,
            "total_tax": 10,
            "total_surcharge": 4,
            "created_at": now,
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "stocks": [{"item_id": "2", "quantity": 20}],
        }

    def test_1_post_order(self):
        response = self.client.post(self.base_url, json=self.order_payload)
        print("POST RESPONSE:", response.text)
        self.assertIn(
            response.status_code, (200, 201), f"Failed to create order: {response.text}"
        )
        data = response.json()
        self.order_id = data.get("id")
        self.assertIsNotNone(self.order_id, "Order ID should be returned")
        self.assertEqual(data.get("reference"), self.order_payload["reference"])

    def test_2_get_order(self):
        if not self.order_id:
            self.skipTest("Create order test failed or not run.")
        response = self.client.get(f"{self.base_url}{self.order_id}")
        self.assertEqual(
            response.status_code, 200, f"Failed to fetch order: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["reference"], self.order_payload["reference"])

    def test_3_update_order(self):
        if not self.order_id:
            self.skipTest("Create order test failed or not run.")
        response = self.client.put(
            f"{self.base_url}{self.order_id}", json=self.updated_order
        )
        self.assertEqual(
            response.status_code, 200, f"Failed to update order: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["reference"], self.updated_order["reference"])

    def test_4_get_all_orders(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_5_soft_delete_order(self):
        if not self.order_id:
            self.skipTest("Create order test failed or not run.")
        response = self.client.delete(f"{self.base_url}{self.order_id}")
        self.assertIn(response.status_code, (200, 204))
        get_response = self.client.get(f"{self.base_url}{self.order_id}")
        self.assertIn(get_response.status_code, (404, 410))

    def tearDown(self):
        if self.order_id:
            self.client.delete(f"{self.base_url}{self.order_id}")
        self.client.close()


if __name__ == "__main__":
    unittest.main()
