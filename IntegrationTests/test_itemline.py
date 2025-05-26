import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout


class TestItemLineEndpoint(unittest.TestCase):
    def setUp(self):
        self.base_url = "http://localhost:5000/api/v1/itemlines/"
        timeout = Timeout(60.0)  #
        self.client = Client(timeout=timeout)
        self.client.headers = {
            "X-Api-Key": "AdminKey123",
            "Content-Type": "application/json",
        }
        self.created_itemline_id = None

        self.test_itemline = {
            "name": "Test Item Line",
            "description": "This is a test description",
            "created_at": datetime.utcnow().isoformat() + "Z",
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "is_deleted": False,
        }

        self.updated_itemline = {
            "name": "Updated Item Line",
            "description": "Updated description",
            "created_at": self.test_itemline["created_at"],
            "updated_at": datetime.utcnow().isoformat() + "Z",
            "is_deleted": False,
        }

    def test_1_post_itemline(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201), f"POST failed: {response.text}")
        data = response.json()
        self.created_itemline_id = data.get("id") or data.get("Id")
        self.assertIsNotNone(self.created_itemline_id, "ID not returned on create")
        self.assertEqual(
            data.get("name") or data.get("Name"), self.test_itemline["name"]
        )

    def test_2_get_itemline(self):
        if not self.created_itemline_id:
            self.skipTest("Create itemline failed or did not run")

        response = self.client.get(f"{self.base_url}{self.created_itemline_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data.get("id") or data.get("Id"), self.created_itemline_id)

    def test_3_update_itemline(self):
        if not self.created_itemline_id:
            self.skipTest("Create itemline failed or did not run")

        response = self.client.put(
            f"{self.base_url}{self.created_itemline_id}", json=self.updated_itemline
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(
            data.get("description") or data.get("Description"),
            self.updated_itemline["description"],
        )

    def test_4_get_all_itemlines(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_itemline(self):
        if not self.created_itemline_id:
            self.skipTest("Create itemline failed or did not run")

        response = self.client.delete(f"{self.base_url}{self.created_itemline_id}")
        self.assertIn(response.status_code, (200, 204))

        get_response = self.client.get(f"{self.base_url}{self.created_itemline_id}")
        self.assertIn(get_response.status_code, (404, 410))

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
