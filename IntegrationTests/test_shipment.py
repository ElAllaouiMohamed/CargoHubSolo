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
            "orderId": 1,
            "sourceId": 99,
            "orderDate": "2025-01-01T00:00:00Z",
            "requestDate": "2025-01-02T00:00:00Z",
            "shipmentDate": "2025-01-03T00:00:00Z",
            "shipmentType": "Express",
            "shipmentStatus": "Pending",
            "notes": "Test shipment",
            "carrierCode": "UPS",
            "carrierDescription": "UPS Services",
            "serviceCode": "EXP",
            "paymentType": "Prepaid",
            "transferMode": "Air",
            "totalPackageCount": 3,
            "totalPackageWeight": 15.5,
            "createdAt": now,
            "updatedAt": now,
            "stocks": [{"itemId": "1", "quantity": 10}],
        }

        self.invalid_shipment = {
            "shipmentType": "Express",
            "shipmentStatus": "Pending",
            "notes": "Invalid shipment",
            "carrierCode": "UPS",
            "carrierDescription": "UPS Services",
            "serviceCode": "EXP",
            "paymentType": "Prepaid",
            "transferMode": "Air",
            "totalPackageCount": -1,  # negatief
            "totalPackageWeight": -10.0,
            "createdAt": now,
            "updatedAt": now,
        }

    def test_1_create_shipment(self):
        response = self.client.post(self.base_url, json=self.test_shipment)
        self.assertIn(
            response.status_code, (200, 201), f"Creation failed: {response.text}"
        )
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(
            data.get("shipmentStatus"), self.test_shipment["shipmentStatus"]
        )

    def test_2_create_invalid_shipment_should_fail(self):
        response = self.client.post(self.base_url, json=self.invalid_shipment)
        self.assertIn(
            response.status_code,
            (400, 422),
            f"Expected validation error: {response.text}",
        )
        data = response.json()
        self.assertIn("errors", data, "Expected validation errors in response")
        errors = str(data["errors"].values())
        self.assertTrue(
            any(
                [
                    "orderId" in errors.lower(),
                    "sourceId" in errors.lower(),
                    "orderDate" in errors.lower(),
                    "requestDate" in errors.lower(),
                    "shipmentDate" in errors.lower(),
                    "stocks" in errors.lower(),
                    "totalPackageCount" in errors.lower(),
                    "totalPackageWeight" in errors.lower(),
                ]
            ),
            "Expected specific validation error messages",
        )

    def test_3_get_all_shipments(self):
        response = self.client.post(self.base_url, json=self.test_shipment)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_4_get_by_id(self):
        response = self.client.post(self.base_url, json=self.test_shipment)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["id"], self.test_id)

    def test_5_update_shipment(self):
        response = self.client.post(self.base_url, json=self.test_shipment)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

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
            "createdAt": self.test_shipment["createdAt"],
            "updatedAt": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
            "stocks": [{"itemId": "1", "quantity": 20}],
        }

        response = self.client.put(f"{self.base_url}{self.test_id}", json=updated)
        self.assertEqual(response.status_code, 200, f"Update failed: {response.text}")
        json_data = response.json()
        self.assertEqual(json_data["shipmentStatus"], "Shipped")
        self.assertEqual(json_data["totalPackageWeight"], 22.0)
        self.assertEqual(json_data["stocks"][0]["quantity"], 20)

    def test_6_soft_delete(self):
        response = self.client.post(self.base_url, json=self.test_shipment)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.test_id = data.get("id")

        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertIn(response.status_code, (204, 200))

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertIn(get_response.status_code, (404, 410))

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()


if __name__ == "__main__":
    unittest.main()
