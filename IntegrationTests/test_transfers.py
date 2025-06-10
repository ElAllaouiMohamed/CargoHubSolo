import unittest
from httpx import Client, Timeout
from datetime import datetime
import os


class TestTransfersEndpoint(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.api_key = os.getenv("TEST_API_KEY", "fallback")
        cls.base_url = "http://localhost:5000/api/v1/transfers/"
        cls.client = Client(
            timeout=Timeout(30.0),
            headers={
                "X-Api-Key": cls.api_key,
                "Content-Type": "application/json",
            },
        )
        now = datetime.utcnow().isoformat() + "Z"
        cls.valid_payload = {
            "reference": "VALID-001",
            "transfer_from": 1,
            "transfer_to": 2,
            "transfer_status": "Pending",
            "created_at": now,
            "updated_at": now,
            "items": [],
            "is_deleted": False,
        }
        cls.invalid_payload = {
            "transfer_from": "INVALID",
            "transfer_to": 2,
            "transfer_status": "Pending",
            "created_at": now,
            "updated_at": now,
            "items": [],
            "is_deleted": False,
        }

    def test_1_create_valid_transfer(self):
        response = self.client.post(self.base_url, json=self.valid_payload)
        self.assertIn(response.status_code, [200, 201])
        data = response.json()
        self.__class__.transfer_id = data["id"]
        self.assertEqual(data["reference"], self.valid_payload["reference"])

    def test_2_create_invalid_transfer_should_fail(self):
        response = self.client.post(self.base_url, json=self.invalid_payload)
        self.assertIn(response.status_code, [400, 422])

    def test_3_get_by_id(self):
        response = self.client.get(f"{self.base_url}{self.transfer_id}")
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["id"], self.transfer_id)

    def test_4_update_transfer(self):
        updated = self.valid_payload.copy()
        updated["reference"] = "UPDATED-REF"
        updated["transferStatus"] = "Completed"

        response = self.client.put(f"{self.base_url}{self.transfer_id}", json=updated)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["reference"], "UPDATED-REF")
        print("UPDATE RESPONSE JSON:", data)

        self.assertEqual(data["transferStatus"], "Completed")

    def test_5_delete_transfer(self):
        response = self.client.delete(f"{self.base_url}{self.transfer_id}")
        self.assertIn(response.status_code, [200, 204])

        get_response = self.client.get(f"{self.base_url}{self.transfer_id}")
        self.assertIn(get_response.status_code, [404, 410])

    @classmethod
    def tearDownClass(cls):
        cls.client.close()


if __name__ == "__main__":
    unittest.main()
