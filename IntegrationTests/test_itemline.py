import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestItemLinesEndpoint(unittest.TestCase):

    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/itemlines/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.test_id = None

        now = datetime.now(timezone.utc).isoformat()

        self.test_itemline = {
            "name": "Test Line",
            "description": "Test item line description",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

        self.updated_itemline = {
            "name": "Updated Line",
            "description": "Updated description",
            "created_at": now,
            "updated_at": now,
            "isDeleted": False,
        }

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_post_itemline(self):
        response = self.client.post(self.base_url, json=self.test_itemline)
        self.assertIn(response.status_code, (200, 201), f"POST failed: {response.text}")
        data = response.json()
        self.test_id = data.get("id")
        self.assertIsNotNone(self.test_id)
        self.assertEqual(data["name"], self.test_itemline["name"])

    def test_2_get_itemline(self):
        if not self.test_id:
            self.skipTest("Create test not run.")
        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)

    def test_3_update_itemline(self):
        if not self.test_id:
            self.skipTest("Create test not run.")
        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_itemline
        )
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json()["name"], self.updated_itemline["name"])

    def test_4_get_all_itemlines(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        self.assertTrue(isinstance(response.json(), list))

    def test_5_soft_delete_itemline(self):
        if not self.test_id:
            self.skipTest("Create test not run.")
        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertIn(response.status_code, [200, 204])
        get_resp = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertIn(get_resp.status_code, [404, 410])

    def test_6_post_invalid_name(self):
        invalid = self.test_itemline.copy()
        invalid["name"] = "Invalid123!"
        response = self.client.post(self.base_url, json=invalid)
        self.assertEqual(response.status_code, 400)
        self.assertIn(
            "Name mag geen cijfers of speciale tekens bevatten", response.text
        )


if __name__ == "__main__":
    unittest.main()
