import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestOrdersEndpoint(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.api_key = os.getenv("TEST_API_KEY", "fallback")
        cls.base_url = "http://localhost:5000/api/v1/orders/"
        cls.client = Client(
            timeout=Timeout(60.0),
            headers={
                "X-Api-Key": cls.api_key,
                "Content-Type": "application/json",
            },
        )

        now = datetime.now(timezone.utc).isoformat()

        cls.valid_payload = {
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

        cls.updated_payload = cls.valid_payload.copy()
        cls.updated_payload["reference"] = "UPDATED-ORDER-001"
        cls.updated_payload["order_status"] = "Shipped"
        cls.updated_payload["items"] = [{"item_id": "2", "quantity": 5}]

        cls.invalid_payload = cls.valid_payload.copy()
        del cls.invalid_payload["reference"]
        cls.invalid_payload["items"] = []  # kan niet leeg zijn

    def test_1_create_order(self):
        response = self.client.post(self.base_url, json=self.valid_payload)
        self.assertIn(response.status_code, [200, 201], f"POST failed: {response.text}")
        data = response.json()
        print("Created Order:", data)
        self.__class__.order_id = data["id"]
        self.assertEqual(data["reference"], self.valid_payload["reference"])

    def test_2_invalid_order_fails(self):
        response = self.client.post(self.base_url, json=self.invalid_payload)
        self.assertIn(
            response.status_code,
            [400, 422],
            f"Validation did not trigger: {response.text}",
        )

    def test_3_get_order_by_id(self):
        response = self.client.get(f"{self.base_url}{self.order_id}")
        self.assertEqual(response.status_code, 200, f"GET failed: {response.text}")
        self.assertEqual(response.json()["reference"], self.valid_payload["reference"])

    def test_4_update_order(self):
        response = self.client.put(
            f"{self.base_url}{self.order_id}", json=self.updated_payload
        )
        self.assertEqual(response.status_code, 200, f"PUT failed: {response.text}")
        data = response.json()
        self.assertEqual(data["reference"], self.updated_payload["reference"])
        self.assertEqual(data["order_status"], self.updated_payload["order_status"])

    def test_5_get_all_orders(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_6_soft_delete_order(self):
        response = self.client.delete(f"{self.base_url}{self.order_id}")
        self.assertIn(response.status_code, [200, 204])
        get_response = self.client.get(f"{self.base_url}{self.order_id}")
        self.assertIn(get_response.status_code, [404, 410])

    @classmethod
    def tearDownClass(cls):
        cls.client.close()


if __name__ == "__main__":
    unittest.main()
