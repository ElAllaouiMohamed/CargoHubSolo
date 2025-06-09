import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestShipmentsEndpoint(unittest.TestCase):
    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/shipments/"
        self.client = Client(
            timeout=Timeout(60.0),
            headers={"X-Api-Key": api_key, "Content-Type": "application/json"},
        )
        self.test_id = None

        now = datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")
        self.test_shipment = {
            "order_id": 1,
            "source_id": 99,
            "order_date": "2025-01-01T00:00:00Z",
            "request_date": "2025-01-02T00:00:00Z",
            "shipment_date": "2025-01-03T00:00:00Z",
            "shipment_type": "Express",
            "shipment_status": "Pending",
            "notes": "Test shipment",
            "carrier_code": "UPS",
            "carrier_description": "UPS Services",
            "service_code": "EXP",
            "payment_type": "Prepaid",
            "transfer_mode": "Air",
            "total_package_count": 3,
            "total_package_weight": 15.5,
            "created_at": now,
            "updated_at": now,
            "stocks": [{"item_id": "1", "quantity": 10}],
        }

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_create_shipment(self):
        response = self.client.post(self.base_url, json=self.test_shipment)
        self.assertIn(
            response.status_code, (200, 201), f"Creation failed: {response.text}"
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)

    def test_2_get_all_shipments(self):
        self.test_1_create_shipment()
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_3_get_by_id(self):
        self.test_1_create_shipment()
        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["id"], self.test_id)

    def test_4_update_shipment(self):
        self.test_1_create_shipment()
        updated = {
            "id": self.test_id,
            "orderId": 1,
            "sourceId": 99,
            "orderDate": "2025-01-01T00:00:00Z",
            "requestDate": "2025-01-02T00:00:00Z",
            "shipmentDate": "2025-01-03T00:00:00Z",
            "shipmentType": "Express",
            "shipmentStatus": "Shipped",
            "notes": "Updated shipment",
            "carrierCode": "UPS",
            "carrierDescription": "UPS Services",
            "serviceCode": "EXP",
            "paymentType": "Prepaid",
            "transferMode": "Air",
            "totalPackageCount": 5,
            "totalPackageWeight": 22.0,
            "createdAt": self.test_shipment["created_at"],
            "updatedAt": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
            "stocks": [{"itemId": "1", "quantity": 20}],
        }

        response = self.client.put(f"{self.base_url}{self.test_id}", json=updated)
        self.assertEqual(response.status_code, 200, f"Update failed: {response.text}")
        json_data = response.json()
        self.assertEqual(json_data["shipmentStatus"], "Shipped")
        self.assertEqual(json_data["totalPackageWeight"], 22.0)
        self.assertEqual(json_data["stocks"][0]["quantity"], 20)

    def test_5_soft_delete(self):
        self.test_1_create_shipment()
        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertIn(response.status_code, (204, 200))


if __name__ == "__main__":
    unittest.main()
