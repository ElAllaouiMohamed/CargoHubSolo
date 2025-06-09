import unittest
from httpx import Client, Timeout
from datetime import datetime, timezone
import os


class TestItemTypesEndpoint(unittest.TestCase):
    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/itemtypes/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )

        now = datetime.now(timezone.utc).isoformat()

        self.test_item_type = {
            "Name": "Test Type",
            "Description": "Test item type description",
            "CreatedAt": now,
            "UpdatedAt": now,
            "IsDeleted": False,
        }

        self.updated_item_type = {
            "Name": "Updated Type",
            "Description": "Updated description",
            "CreatedAt": now,
            "UpdatedAt": datetime.now(timezone.utc).isoformat(),
            "IsDeleted": False,
        }

        response = self.client.get(self.base_url)
        if response.status_code == 200:
            for entry in response.json():
                if entry["name"] in ["Test Type", "Updated Type"]:
                    self.client.delete(f"{self.base_url}{entry['id']}")

        post_response = self.client.post(self.base_url, json=self.test_item_type)
        assert post_response.status_code in [
            200,
            201,
        ], f"Setup failed: {post_response.text}"
        self.test_id = post_response.json()["id"]

    def tearDown(self):
        if self.test_id:
            self.client.delete(f"{self.base_url}{self.test_id}")
        self.client.close()

    def test_1_post_item_type(self):
        self.assertIsNotNone(self.test_id)

    def test_2_get_item_type(self):
        response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["id"], self.test_id)
        self.assertEqual(data["name"], self.test_item_type["Name"])
        self.assertEqual(data["description"], self.test_item_type["Description"])

    def test_3_update_item_type(self):
        response = self.client.put(
            f"{self.base_url}{self.test_id}", json=self.updated_item_type
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data["name"], self.updated_item_type["Name"])
        self.assertEqual(data["description"], self.updated_item_type["Description"])
        self.assertFalse(data["isDeleted"])

    def test_4_get_all_item_types(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertTrue(isinstance(data, list))
        found = any(item["id"] == self.test_id for item in data)
        self.assertTrue(found, "Created item type should be in the list")

    def test_5_soft_delete_item_type(self):
        response = self.client.delete(f"{self.base_url}{self.test_id}")
        self.assertEqual(response.status_code, 204)

        get_response = self.client.get(f"{self.base_url}{self.test_id}")
        self.assertEqual(get_response.status_code, 404)


if __name__ == "__main__":
    unittest.main()
