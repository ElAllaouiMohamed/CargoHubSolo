import unittest
from httpx import Client
from httpx import Timeout
import os


class TestClientsEndpoint(unittest.TestCase):
    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/clients/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.TEST_CLIENT_ID = None

        self.TEST_CLIENT = {
            "name": "Integration Test Client",
            "address": "123 Test St",
            "city": "Test City",
            "zip_code": "12345",
            "province": "Test Province",
            "country": "Test Country",
            "contact_name": "Tester",
            "contact_phone": "+1234567890",
            "contact_email": "tester@example.com",
        }

        self.UPDATE_CLIENT = {
            "name": "Updated Client Name",
            "address": "321 Updated St",
            "city": "Updated City",
            "zip_code": "54321",
            "province": "Updated Province",
            "country": "Updated Country",
            "contact_name": "Updated Tester",
            "contact_phone": "+0987654321",
            "contact_email": "updated@example.com",
        }

    def test_1_create_client(self):
        response = self.client.post(self.base_url, json=self.TEST_CLIENT)
        self.assertIn(response.status_code, [200, 201])
        json_resp = response.json()
        self.TEST_CLIENT_ID = json_resp.get("id")
        self.assertIsNotNone(self.TEST_CLIENT_ID)
        self.assertEqual(json_resp["name"], self.TEST_CLIENT["name"])

    def test_2_get_client_by_id(self):

        if not self.TEST_CLIENT_ID:
            self.skipTest("Create client test failed or not run.")

        response = self.client.get(f"{self.base_url}{self.TEST_CLIENT_ID}")
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(json_resp["id"], self.TEST_CLIENT_ID)
        self.assertEqual(json_resp["name"], self.TEST_CLIENT["name"])

    def test_3_update_client(self):
        if not self.TEST_CLIENT_ID:
            self.skipTest("Create client test failed or not run.")

        response = self.client.put(
            f"{self.base_url}{self.TEST_CLIENT_ID}", json=self.UPDATE_CLIENT
        )
        self.assertEqual(response.status_code, 200)
        json_resp = response.json()
        self.assertEqual(json_resp["name"], self.UPDATE_CLIENT["name"])

    def test_4_delete_client(self):
        if not self.TEST_CLIENT_ID:
            self.skipTest("Create client test failed or not run.")

        response = self.client.delete(f"{self.base_url}{self.TEST_CLIENT_ID}")
        self.assertIn(response.status_code, [200, 204])

        # Controleer dat client is verwijderd (soft delete: endpoint zou 404 kunnen geven)
        response_get = self.client.get(f"{self.base_url}{self.TEST_CLIENT_ID}")
        self.assertIn(response_get.status_code, [404, 410])

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
