import unittest
from httpx import Client
from datetime import datetime

class TestItemLineEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/itemlines/"
        self.client = Client()
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json"
        }
        self.test_id = 9999

        self.test_itemline = {
            "id": self.test_id,
            "name": "Test Item Line",
            "description": "This is a test description",
            "created_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "is_deleted": False
        }

        self.updated_itemline = {
            "name": "Updated Item Line",
            "description": "Updated description",
            "created_at": self.test_itemline["created_at"],
            "updated_at": datetime.utcnow().strftime("%Y-%m-%d %H:%M:%S"),
            "is_deleted": False
        }

    def test_1_post_itemline(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201))
        data = response.json()
        self.assertEqual(data["name"], self.test_itemline["name"])

    def test_2_get_itemline(self):
        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)

    def test_3_update_itemline(self):
        response = self.client.put(f"{self.base_url}{self.test_id}", json=self.updated_itemline)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["description"], self.updated_itemline["description"])

    def test_4_get_all_itemlines(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertTrue(isinstance(data, list))

    def test_5_soft_delete_itemline(self):
        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 204)  # No Content expected on delete

        # Verify that deleted itemline no longer accessible
        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(get_response.status_code, 404)  # Not found

if __name__ == "__main__":
    unittest.main()
