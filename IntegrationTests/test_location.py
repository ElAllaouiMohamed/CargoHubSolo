import unittest
from httpx import Client
from datetime import datetime
from httpx import Timeout
import os


class TestLocationEndpoint(unittest.TestCase):

    def setUp(self):
        api_key = os.getenv("TEST_API_KEY", "fallback")
        self.base_url = "http://localhost:5000/api/v1/locations/"
        timeout = Timeout(60.0)
        self.client = Client(
            timeout=timeout,
            headers={
                "X-Api-Key": api_key,
                "Content-Type": "application/json",
            },
        )
        self.created_location_id = None

        now = datetime.utcnow().isoformat() + "Z"

        self.test_location = {
            "WarehouseId": 1,
            "Code": "LOC001",
            "Name": "Test Location",
            "CreatedAt": now,
            "UpdatedAt": now,
            "IsDeleted": False,
        }

        self.updated_location = {
            "WarehouseId": 2,
            "Code": "LOC002",
            "Name": "Updated Location",
            "CreatedAt": now,
            "UpdatedAt": datetime.utcnow().isoformat() + "Z",
            "IsDeleted": False,
        }

    def test_1_create_location(self):
        response = self.client.post(self.base_url, json=self.test_location)
        self.assertIn(response.status_code, (200, 201), f"POST failed: {response.text}")
        data = response.json()
        self.created_location_id = data.get("id") or data.get("Id")
        self.assertIsNotNone(self.created_location_id, "No ID returned from POST")
        self.assertEqual(
            data.get("Code") or data.get("code"), self.test_location["Code"]
        )

    def test_2_get_location_by_id(self):
        if not self.created_location_id:
            self.skipTest("Create location failed or did not run")
        response = self.client.get(f"{self.base_url}{self.created_location_id}")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(data.get("id") or data.get("Id"), self.created_location_id)

    def test_3_update_location(self):
        if not self.created_location_id:
            self.skipTest("Create location failed or did not run")
        response = self.client.put(
            f"{self.base_url}{self.created_location_id}", json=self.updated_location
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertEqual(
            data.get("Name") or data.get("name"), self.updated_location["Name"]
        )

    def test_4_get_all_locations(self):
        response = self.client.get(self.base_url)
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIsInstance(data, list)

    def test_5_soft_delete_location(self):
        if not self.created_location_id:
            self.skipTest("Create location failed or did not run")
        response = self.client.delete(f"{self.base_url}{self.created_location_id}")
        self.assertIn(response.status_code, (200, 204))

        get_response = self.client.get(f"{self.base_url}{self.created_location_id}")
        self.assertIn(get_response.status_code, (404, 410))

    def tearDown(self):
        self.client.close()


if __name__ == "__main__":
    unittest.main()
