import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestTransfersEndpoint(unittest.TestCase):
    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/transfers/"
        self.client = Client(
            timeout=Timeout(60.0),
            headers={"X-Api-Key": api_key, "Content-Type": "application/json"},
        )
        self.created_transfer_id = None

        now = datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")
        self.test_transfer = {
            "reference": "TRF-001",
            "transfer_from": 100,
            "transfer_to": 200,
            "transfer_status": "Planned",
            "created_at": now,
            "updated_at": now,
            "items": [{"item_id": "ITM-001", "amount": 5}],
        }

        self.updated_transfer = {
            "reference": "TRF-001-UPDATED",
            "transfer_from": 300,
            "transfer_to": 400,
            "transfer_status": "Completed",
            "created_at": now,
            "updated_at": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
            "items": [{"item_id": "ITM-002", "amount": 10}],
        }

    def tearDown(self):
        if self.created_transfer_id:
            self.client.delete(f"{self.base_url}{self.created_transfer_id}")
        self.client.close()

    def test_1_create_transfer(self):
        response = self.client.post(self.base_url, json=self.test_transfer)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.created_transfer_id = data.get("id")
        self.assertIsNotNone(self.created_transfer_id)
        self.assertEqual(data["reference"], self.test_transfer["reference"])

    def test_2_get_by_id(self):
        self.test_1_create_transfer()
        response = self.client.get(f"{self.base_url}{self.created_transfer_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.created_transfer_id)

    def test_3_update_transfer(self):
        if not self.created_transfer_id:
            self.skipTest("Create transfer test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.created_transfer_id}", json=self.updated_transfer
        )
        self.assertEqual(response.status_code, 200, f"PUT failed: {response.text}")
        data = response.json()
        print("RESPONSE JSON:", data)

        self.assertEqual(
            data["transferStatus"],
            self.updated_transfer["transfer_status"],
            f"Expected 'transfer_status' to be '{self.updated_transfer['transfer_status']}' but got: {data.get('transferStatus')}",
        )

    def test_4_get_all(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertIsInstance(response.json(), list)

    def test_5_soft_delete(self):
        self.test_1_create_transfer()
        response = self.client.delete(f"{self.base_url}{self.created_transfer_id}")
        self.assertIn(response.status_code, [200, 204])

        follow_up = self.client.get(f"{self.base_url}{self.created_transfer_id}")
        self.assertIn(follow_up.status_code, [404, 410])


if __name__ == "__main__":
    unittest.main()
