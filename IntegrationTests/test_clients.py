import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestClientsEndpoint(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        cls.base_url = "http://localhost:5000/api/v1/clients/"
        timeout = Timeout(60.0)
        cls.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        cls.created_id = None

        now = datetime.now(timezone.utc).isoformat()

        cls.valid_client = {
            "Name": "Integration Test Client",
            "Address": "123 Test St",
            "City": "Test City",
            "ZipCode": "12345",
            "Province": "Test Province",
            "Country": "Test Country",
            "ContactName": "Tester",
            "ContactPhone": "+1234567890",
            "ContactEmail": "tester@example.com",
            "CreatedAt": now,
            "UpdatedAt": now,
        }

        cls.updated_client = cls.valid_client.copy()
        cls.updated_client["Name"] = "Updated Client"
        cls.updated_client["ContactEmail"] = "updated@example.com"

        cls.invalid_client = cls.valid_client.copy()
        del cls.invalid_client["ContactEmail"]
        cls.invalid_client["ContactPhone"] = "INVALID PHONE"  # fout formaat

    def test_1_create_client(self):
        response = self.client.post(self.base_url, json=self.valid_client)
        self.assertIn(
            response.status_code, [200, 201], f"Create failed: {response.text}"
        )
        data = response.json()
        TestClientsEndpoint.created_id = data["id"]
        self.assertEqual(
            data.get("name") or data.get("Name"), self.valid_client["Name"]
        )

    def test_2_create_invalid_client(self):
        response = self.client.post(self.base_url, json=self.invalid_client)
        self.assertIn(
            response.status_code,
            [400, 422],
            f"Invalid create not rejected: {response.text}",
        )

    def test_3_get_by_id(self):
        response = self.client.get(f"{self.base_url}{self.created_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data.get("id") or data.get("Id"), self.created_id)

    def test_4_update_client(self):
        response = self.client.put(
            f"{self.base_url}{self.created_id}", json=self.updated_client
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        print("json test:", data)
        self.assertEqual(data.get("contactEmail"), self.updated_client["ContactEmail"])

    def test_5_soft_delete_client(self):
        response = self.client.delete(f"{self.base_url}{self.created_id}")
        self.assertIn(response.status_code, [200, 204])
        check = self.client.get(f"{self.base_url}{self.created_id}")
        self.assertIn(check.status_code, [404, 410])

    @classmethod
    def tearDownClass(cls):
        cls.client.close()


if __name__ == "__main__":
    unittest.main()
