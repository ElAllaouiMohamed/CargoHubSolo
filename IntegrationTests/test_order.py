import unittest
import httpx
from datetime import datetime

BASE_URL = "http://localhost:5000/api/v1/orders"
HEADERS = {
    "accept": "*/*",
    "Content-Type": "application/json",
    "X-Api-Key": "AdminKey123",
}


class TestOrdersEndpoint(unittest.TestCase):

    def setUp(self):
        self.order_payload = {
            "source_id": 1,
            "order_date": datetime.utcnow().isoformat() + "Z",
            "request_date": datetime.utcnow().isoformat() + "Z",
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
            "created_at": datetime.utcnow().isoformat() + "Z",
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "stocks": [{"item_id": "1", "quantity": 10}],
        }

    def test_1_post_order(self):
        response = httpx.post(BASE_URL, headers=HEADERS, json=self.order_payload)
        print("POST RESPONSE:", response.text)
        self.assertIn(
            response.status_code, (200, 201), f"Failed to create order: {response.text}"
        )
        self.created_order = response.json()
        self.order_id = self.created_order["id"]

    def test_2_get_order(self):
        self.test_1_post_order()
        response = httpx.get(f"{BASE_URL}/{self.order_id}", headers=HEADERS)
        self.assertEqual(
            response.status_code, 200, f"Failed to fetch order: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["reference"], self.order_payload["reference"])

    def test_3_update_order(self):
        self.test_1_post_order()
        update_payload = self.order_payload.copy()
        update_payload["reference"] = "UPDATED-REF"
        update_payload["updated_at"] = datetime.utcnow().isoformat() + "Z"
        response = httpx.put(
            f"{BASE_URL}/{self.order_id}", headers=HEADERS, json=update_payload
        )
        self.assertEqual(
            response.status_code, 200, f"Failed to update order: {response.text}"
        )
        data = response.json()
        self.assertEqual(data["reference"], "UPDATED-REF")

    def test_4_get_all_orders(self):
        response = httpx.get(BASE_URL, headers=HEADERS)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_5_soft_delete_order(self):
        self.test_1_post_order()
        response = httpx.delete(f"{BASE_URL}/{self.order_id}", headers=HEADERS)
        self.assertIn(response.status_code, (200, 204))
        self.assertIn(response.status_code, (200, 204))


if __name__ == "__main__":
    unittest.main()
