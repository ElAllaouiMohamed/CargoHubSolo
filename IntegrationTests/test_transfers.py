import unittest
from httpx import Client
from datetime import datetime

class TestTransfersEndpoint(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.base_url = "http://localhost:5000/api/v1/transfers/"
        cls.client = Client(timeout=30.0)
        cls.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json"
        }
        cls.created_transfer_id = None

    def test_1_create_transfer(self):
        test_transfer = {
            "reference": "REF-123456",
            "transfer_from": 1,
            "transfer_to": 2,
            "transfer_status": "Pending",
            "items": [],
            "is_deleted": False
        }
        response = self.client.post(self.base_url, json=test_transfer)
        self.assertIn(response.status_code, (200, 201), f"Response: {response.text}")
        data = response.json()
        self.assertEqual(data["reference"], test_transfer["reference"])
        self.assertIsNotNone(data.get("id"))
        TestTransfersEndpoint.created_transfer_id = data["id"]

    def test_2_get_transfer_by_id(self):
        self.assertIsNotNone(TestTransfersEndpoint.created_transfer_id, "Maak eerst transfer aan in test_1_create_transfer")
        response = self.client.get(f"{self.base_url}{TestTransfersEndpoint.created_transfer_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], TestTransfersEndpoint.created_transfer_id)

    def test_3_update_transfer(self):
        self.assertIsNotNone(TestTransfersEndpoint.created_transfer_id, "Maak eerst transfer aan in test_1_create_transfer")
        updated_transfer = {
            "reference": "REF-654321",
            "transfer_from": 3,
            "transfer_to": 4,
            "transfer_status": "Completed",
            "items": [],
            "is_deleted": False
        }
        response = self.client.put(f"{self.base_url}{TestTransfersEndpoint.created_transfer_id}", json=updated_transfer)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["transfer_status"], updated_transfer["transfer_status"])

    def test_4_get_all_transfers(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_transfer(self):
        self.assertIsNotNone(TestTransfersEndpoint.created_transfer_id, "Maak eerst transfer aan in test_1_create_transfer")
        response = self.client.delete(f"{self.base_url}{TestTransfersEndpoint.created_transfer_id}")
        self.assertEqual(response.status_code, 204)

        # Controleer of soft delete werkt: GET moet 404 geven
        get_response = self.client.get(f"{self.base_url}{TestTransfersEndpoint.created_transfer_id}")
        self.assertEqual(get_response.status_code, 404)


if __name__ == "__main__":
    unittest.main()
