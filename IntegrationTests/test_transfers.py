import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout


class TestTransfersEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/transfers/"
        timeout = Timeout(60.0)  # 15 seconden timeout
        self.client = Client(timeout=timeout)
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json",
        }
        self.created_transfer_id = None

        self.test_transfer = {
            "reference": "REF-123456",
            "transfer_from": 1,
            "transfer_to": 2,
            "transfer_status": "Pending",
            "created_at": datetime.utcnow().isoformat() + "Z",
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "items": [],
            "is_deleted": False,
        }

        self.updated_transfer = {
            "reference": "REF-654321",
            "transfer_from": 3,
            "transfer_to": 4,
            "transfer_status": "Completed",
            "items": [],
            "is_deleted": False,
        }

    def test_1_create_transfer(self):
        response = self.client.post(self.base_url, json=self.test_transfer)
        self.assertIn(response.status_code, [200, 201])
        json_resp = response.json()
        self.created_transfer_id = json_resp.get("id")
        self.assertIsNotNone(self.created_transfer_id)
        self.assertEqual(json_resp["reference"], self.test_transfer["reference"])

    def test_2_get_transfer_by_id(self):
        if not self.created_transfer_id:
            self.skipTest("Create transfer test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.created_transfer_id}")
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(json_resp["id"], self.created_transfer_id)
        self.assertEqual(json_resp["reference"], self.test_transfer["reference"])

    def test_3_update_transfer(self):
        if not self.created_transfer_id:
            self.skipTest("Create transfer test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.created_transfer_id}", json=self.updated_transfer
        )
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(
            json_resp["transfer_status"], self.updated_transfer["transfer_status"]
        )
        self.assertEqual(json_resp["reference"], self.updated_transfer["reference"])

    def test_4_get_all_transfers(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertIsInstance(json_resp, list)

    def test_5_soft_delete_transfer(self):
        if not self.created_transfer_id:
            self.skipTest("Create transfer test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.created_transfer_id}")
        self.assertIn(response.status_code, [200, 204])

        # Check that transfer is soft deleted (should return 404 or similar)
        response_get = self.client.get(f"{self.base_url}{self.created_transfer_id}")
        self.assertIn(response_get.status_code, [404, 410])

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
