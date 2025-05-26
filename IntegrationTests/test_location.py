import unittest
from httpx import Client
from datetime import datetime

class TestLocationEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/locations/"
        self.client = Client()
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json"
        }
        self.test_id = 9999

        self.test_location = {
            "id": self.test_id,
            "warehouse_id": 1,
            "code": "LOC001",
            "name": "Test Location",
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "is_deleted": False
        }

        self.updated_location = {
            "warehouse_id": 2,
            "code": "LOC002",
            "name": "Updated Location",
            "created_at": self.test_location["created_at"],
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "is_deleted": False
        }

    def test_1_create_location(self):
        response = self.client.post(self.base_url, json=self.test_location)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.assertEqual(data["code"], self.test_location["code"])

    def test_2_get_location_by_id(self):
        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)

    def test_3_update_location(self):
        response = self.client.put(f"{self.base_url}{self.test_id}", json=self.updated_location)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["name"], self.updated_location["name"])

    def test_4_get_all_locations(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_location(self):
        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 204)  # No Content

        # Verify the location is soft deleted (404 on get)
        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(get_response.status_code, 404)

if __name__ == "__main__":
    unittest.main()
