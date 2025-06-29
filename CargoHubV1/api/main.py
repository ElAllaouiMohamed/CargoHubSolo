import socketserver
import http.server
import json
import os

from providers import auth_provider
from providers import data_provider

from processors import notification_processor

from models.clients import Clients
from models.orders import Orders
from models.items import Items
from models.item_groups import ItemGroups
from models.item_lines import ItemLines
from models.locations import Locations
from models.suppliers import Suppliers
from models.transfers import Transfers
from models.warehouses import Warehouses
from models.inventories import Inventories
from models.shipments import Shipments
from models.item_types import ItemTypes

clients_instance = Clients(root_path="./models/", is_debug=False)
warehouses_instance = Warehouses(root_path="./models/", is_debug=False)
locations_instance = Locations(root_path="./models/", is_debug=False)
transfers_instance = Transfers(root_path="./models/", is_debug=False)
items_instance = Items(root_path="./models/", is_debug=False)
item_lines_instance = ItemLines(root_path="./models/", is_debug=False)
item_groups_instance = ItemGroups(root_path="./models/", is_debug=False)
item_types_instance = ItemTypes(root_path="./models/", is_debug=False)
inventories_instance = Inventories(root_path="./models/", is_debug=False)
suppliers_instance = Suppliers(root_path="./models/", is_debug=False)
orders_instance = Orders(root_path="./models/", is_debug=False)
shipments_instance = Shipments(root_path="./models/", is_debug=False)


class ApiRequestHandler(http.server.BaseHTTPRequestHandler):

    def handle_get_version_1(self, path, user):
        if not auth_provider.has_access(user, path, "get"):
            self.send_response(403)
            self.end_headers()
            return
        if path[0] == "warehouses":
            paths = len(path)
            match paths:
                case 1:
                    warehouses = data_provider.fetch_warehouse_pool().get_warehouses()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(warehouses).encode("utf-8"))
                case 2:
                    warehouse_id = int(path[1])
                    warehouse = data_provider.fetch_warehouse_pool().get_warehouse(
                        warehouse_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(warehouse).encode("utf-8"))
                case 3:
                    warehouse_id = int(path[1])
                    data = data_provider.fetch_warehouse_pool().get_warehouse_data(
                        warehouse_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "locations":
            paths = len(path)
            match paths:
                case 1:
                    locations = data_provider.fetch_location_pool().get_locations()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(locations).encode("utf-8"))
                case 2:
                    location_id = int(path[1])
                    location = data_provider.fetch_location_pool().get_location(
                        location_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(location).encode("utf-8"))
                case 3:
                    location_id = int(path[1])
                    data = data_provider.fetch_location_pool().get_location_data(
                        location_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "transfers":
            paths = len(path)
            match paths:
                case 1:
                    transfers = data_provider.fetch_transfer_pool().get_transfers()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(transfers).encode("utf-8"))
                case 2:
                    transfer_id = int(path[1])
                    transfer = data_provider.fetch_transfer_pool().get_transfer(
                        transfer_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(transfer).encode("utf-8"))
                case 3:
                    transfer_id = int(path[1])
                    data = data_provider.fetch_transfer_pool().get_transfer_data(
                        transfer_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "items":
            paths = len(path)
            match paths:
                case 1:
                    items = data_provider.fetch_item_pool().get_items()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(items).encode("utf-8"))
                case 2:
                    item_id = path[1]
                    item = data_provider.fetch_item_pool().get_item(item_id)
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(item).encode("utf-8"))
                case 3:
                    item_id = path[1]
                    data = data_provider.fetch_item_pool().get_item_data(
                        item_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case 4:
                    if path[2] == "inventory" and path[3] == "totals":
                        item_id = path[1]
                        totals = data_provider.fetch_inventory_pool().get_inventory_totals_for_item(
                            item_id
                        )
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(totals).encode("utf-8"))
                    else:
                        self.send_response(404)
                        self.end_headers()
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "item_lines":
            paths = len(path)
            print(paths, path)

            match paths:
                case 1:
                    item_lines = data_provider.fetch_item_line_pool().get_item_lines()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(item_lines).encode("utf-8"))
                case 2:
                    item_line_id = int(path[1])
                    item_line = data_provider.fetch_item_line_pool().get_item_line(
                        item_line_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(item_line).encode("utf-8"))
                case 3:
                    item_line_id = int(path[1])
                    data = data_provider.fetch_item_line_pool().get_item_line_data(
                        item_line_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "item_groups":
            paths = len(path)
            match paths:
                case 1:
                    item_groups = (
                        data_provider.fetch_item_group_pool().get_item_groups()
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(item_groups).encode("utf-8"))
                case 2:
                    item_group_id = int(path[1])
                    item_group = data_provider.fetch_item_group_pool().get_item_group(
                        item_group_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(item_group).encode("utf-8"))
                case 3:
                    item_group_id = int(path[1])
                    data = data_provider.fetch_item_group_pool().get_item_group_data(
                        item_group_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "item_types":
            paths = len(path)
            match paths:
                case 1:
                    item_types = data_provider.fetch_item_type_pool().get_item_types()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(item_types).encode("utf-8"))
                case 2:
                    item_type_id = int(path[1])
                    item_type = data_provider.fetch_item_type_pool().get_item_type(
                        item_type_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(item_type).encode("utf-8"))
                case 3:
                    item_type_id = int(path[1])
                    data = data_provider.fetch_item_type_pool().get_item_type_data(
                        item_type_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "inventories":
            paths = len(path)
            match paths:
                case 1:
                    inventories = data_provider.fetch_inventory_pool().get_inventories()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(inventories).encode("utf-8"))
                case 2:
                    inventory_id = int(path[1])
                    inventory = data_provider.fetch_inventory_pool().get_inventory(
                        inventory_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(inventory).encode("utf-8"))
                case 3:
                    inventory_id = int(path[1])
                    data = data_provider.fetch_inventory_pool().get_inventory_data(
                        inventory_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "suppliers":
            paths = len(path)
            match paths:
                case 1:
                    suppliers = data_provider.fetch_supplier_pool().get_suppliers()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(suppliers).encode("utf-8"))
                case 2:
                    supplier_id = int(path[1])
                    supplier = data_provider.fetch_supplier_pool().get_supplier(
                        supplier_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(supplier).encode("utf-8"))
                case 3:
                    supplier_id = int(path[1])
                    data = data_provider.fetch_supplier_pool().get_supplier_data(
                        supplier_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "orders":
            paths = len(path)
            match paths:
                case 1:
                    orders = data_provider.fetch_order_pool().get_orders()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(orders).encode("utf-8"))
                case 2:
                    order_id = int(path[1])
                    order = data_provider.fetch_order_pool().get_order(order_id)
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(order).encode("utf-8"))
                case 3:
                    order_id = int(path[1])
                    data = data_provider.fetch_order_pool().get_order_data(
                        order_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "clients":
            paths = len(path)
            match paths:
                case 1:
                    clients = data_provider.fetch_client_pool().get_clients()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(clients).encode("utf-8"))
                case 2:
                    client_id = int(path[1])
                    client = data_provider.fetch_client_pool().get_client(client_id)
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(client).encode("utf-8"))
                case 3:
                    client_id = int(path[1])
                    data = data_provider.fetch_client_pool().get_client_data(
                        client_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "shipments":
            paths = len(path)
            match paths:
                case 1:
                    shipments = data_provider.fetch_shipment_pool().get_shipments()
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(shipments).encode("utf-8"))
                case 2:
                    shipment_id = int(path[1])
                    shipment = data_provider.fetch_shipment_pool().get_shipment(
                        shipment_id
                    )
                    self.send_response(200)
                    self.send_header("Content-type", "application/json")
                    self.end_headers()
                    self.wfile.write(json.dumps(shipment).encode("utf-8"))
                case 3:
                    shipment_id = int(path[1])
                    data = data_provider.fetch_shipment_pool().get_shipment_data(
                        shipment_id, path[2]
                    )
                    if data == None:
                        self.send_response(
                            404,
                            "The data you're trying to reach doesn't exist or is located elsewhere. Please try again.",
                        )
                        self.end_headers()
                    else:
                        self.send_response(200)
                        self.send_header("Content-type", "application/json")
                        self.end_headers()
                        self.wfile.write(json.dumps(data).encode("utf-8"))
                case _:
                    self.send_response(404)
                    self.end_headers()
        else:
            self.send_response(404)
            self.end_headers()

    def do_GET(self):
        api_key = self.headers.get("API_KEY")
        user = auth_provider.get_user(api_key)
        if user == None:
            self.send_response(401)
            self.end_headers()
        else:
            try:
                path = self.path.split("/")
                if len(path) > 3 and path[1] == "api" and path[2] == "v1":
                    self.handle_get_version_1(path[3:], user)
            except Exception as e:
                print(f"Exception occurred: {e}")
                self.send_response(500)
                self.end_headers()

    def handle_post_version_1(self, path, user):
        if not auth_provider.has_access(user, path, "post"):
            self.send_response(403)
            self.end_headers()
            return
        if path[0] == "warehouses":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_warehouse = json.loads(post_data.decode())
            data_provider.fetch_warehouse_pool().add_warehouse(new_warehouse)
            data_provider.fetch_warehouse_pool().save()
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_warehouse).encode("utf-8"))

        elif path[0] == "locations":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_location = json.loads(post_data.decode())
            data_provider.fetch_location_pool().add_location(new_location)
            data_provider.fetch_location_pool().save()
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_location).encode("utf-8"))

        elif path[0] == "transfers":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_transfer = json.loads(post_data.decode())
            data_provider.fetch_transfer_pool().add_transfer(new_transfer)
            data_provider.fetch_transfer_pool().save()
            notification_processor.push(
                f"Scheduled batch transfer {new_transfer['id']}"
            )
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_transfer).encode("utf-8"))

        elif path[0] == "items":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_item = json.loads(post_data.decode())
            data_provider.fetch_item_pool().add_item(new_item)
            data_provider.fetch_item_pool().save()
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_item).encode("utf-8"))

        elif path[0] == "inventories":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_inventory = json.loads(post_data.decode())
            data_provider.fetch_inventory_pool().add_inventory(new_inventory)
            data_provider.fetch_inventory_pool().save()
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_inventory).encode("utf-8"))

        elif path[0] == "suppliers":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_supplier = json.loads(post_data.decode())
            data_provider.fetch_supplier_pool().add_supplier(new_supplier)
            data_provider.fetch_supplier_pool().save()
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_supplier).encode("utf-8"))

        elif path[0] == "orders":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_order = json.loads(post_data.decode())
            data_provider.fetch_order_pool().add_order(new_order)
            data_provider.fetch_order_pool().save()
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_order).encode("utf-8"))

        elif path[0] == "clients":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_client = json.loads(post_data.decode())
            data_provider.fetch_client_pool().add_client(new_client)
            data_provider.fetch_client_pool().save()
            self.send_response(201)
            self.send_header("Content-type", "application/json")
            self.end_headers()
            # Zorg ervoor dat de clientgegevens worden weergegeven
            self.wfile.write(json.dumps(new_client).encode("utf-8"))

        elif path[0] == "item_groups":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_item_group = json.loads(post_data.decode())
            data_provider.fetch_item_group_pool().add_item_group(new_item_group)
            data_provider.fetch_item_group_pool().save()
            self.send_response(201)
            self.send_header("Content-type", "application/json")
            self.end_headers()
            # Return the added item group data
            self.wfile.write(json.dumps(new_item_group).encode("utf-8"))

        elif path[0] == "item_lines":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_item_line = json.loads(post_data.decode())
            data_provider.fetch_item_line_pool().add_item_line(new_item_line)
            data_provider.fetch_item_line_pool().save()
            self.send_response(201)
            self.send_header("Content-type", "application/json")
            self.end_headers()
            # Return the added item line data
            self.wfile.write(json.dumps(new_item_line).encode("utf-8"))

        elif path[0] == "item_types":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_item_type = json.loads(post_data.decode())
            data_provider.fetch_item_type_pool().add_item_type(new_item_type)
            data_provider.fetch_item_type_pool().save()
            self.send_response(201)
            self.send_header("Content-type", "application/json")
            self.end_headers()
            # Return the added item type data
            self.wfile.write(json.dumps(new_item_type).encode("utf-8"))

        elif path[0] == "shipments":
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            new_shipment = json.loads(post_data.decode())
            data_provider.fetch_shipment_pool().add_shipment(new_shipment)
            data_provider.fetch_shipment_pool().save()
            self.send_response(201)
            self.end_headers()
            self.wfile.write(json.dumps(new_shipment).encode("utf-8"))

        else:
            self.send_response(404)
            self.end_headers()
            self.wfile.write(json.dumps(new_shipment).encode("utf-8"))

    def do_POST(self):
        api_key = self.headers.get("API_KEY")
        user = auth_provider.get_user(api_key)
        if user == None:
            self.send_response(401)
            self.end_headers()
        else:
            try:
                path = self.path.split("/")
                if len(path) > 3 and path[1] == "api" and path[2] == "v1":
                    self.handle_post_version_1(path[3:], user)
            except Exception:
                self.send_response(500)
                self.end_headers()

    def handle_put_version_1(self, path, user):
        if not auth_provider.has_access(user, path, "put"):
            self.send_response(403)
            self.end_headers()
            return
        if path[0] == "warehouses":
            warehouse_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_warehouse = json.loads(post_data.decode())
            data_provider.fetch_warehouse_pool().update_warehouse(
                warehouse_id, updated_warehouse
            )
            data_provider.fetch_warehouse_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "locations":
            location_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_location = json.loads(post_data.decode())
            data_provider.fetch_location_pool().update_location(
                location_id, updated_location
            )
            data_provider.fetch_location_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "transfers":
            paths = len(path)
            match paths:
                case 2:
                    transfer_id = int(path[1])
                    content_length = int(self.headers["Content-Length"])
                    post_data = self.rfile.read(content_length)
                    updated_transfer = json.loads(post_data.decode())
                    data_provider.fetch_transfer_pool().update_transfer(
                        transfer_id, updated_transfer
                    )
                    data_provider.fetch_transfer_pool().save()
                    self.send_response(200)
                    self.end_headers()
                case 3:
                    if path[2] == "commit":
                        transfer_id = int(path[1])
                        transfer = data_provider.fetch_transfer_pool().get_transfer(
                            transfer_id
                        )
                        for x in transfer["items"]:
                            inventories = data_provider.fetch_inventory_pool().get_inventories_for_item(
                                x["item_id"]
                            )
                            for y in inventories:
                                if y["location_id"] == transfer["transfer_from"]:
                                    y["total_on_hand"] -= x["amount"]
                                    y["total_expected"] = (
                                        y["total_on_hand"] + y["total_ordered"]
                                    )
                                    y["total_available"] = (
                                        y["total_on_hand"] - y["total_allocated"]
                                    )
                                    data_provider.fetch_inventory_pool().update_inventory(
                                        y["id"], y
                                    )
                                elif y["location_id"] == transfer["transfer_to"]:
                                    y["total_on_hand"] += x["amount"]
                                    y["total_expected"] = (
                                        y["total_on_hand"] + y["total_ordered"]
                                    )
                                    y["total_available"] = (
                                        y["total_on_hand"] - y["total_allocated"]
                                    )
                                    data_provider.fetch_inventory_pool().update_inventory(
                                        y["id"], y
                                    )
                        transfer["transfer_status"] = "Processed"
                        data_provider.fetch_transfer_pool().update_transfer(
                            transfer_id, transfer
                        )
                        notification_processor.push(
                            f"Processed batch transfer with id:{transfer['id']}"
                        )
                        data_provider.fetch_transfer_pool().save()
                        data_provider.fetch_inventory_pool().save()
                        self.send_response(200)
                        self.end_headers()
                    else:
                        self.send_response(404)
                        self.end_headers()
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "items":
            item_id = path[1]
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_item = json.loads(post_data.decode())
            data_provider.fetch_item_pool().update_item(item_id, updated_item)
            data_provider.fetch_item_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "item_lines":
            item_line_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_item_line = json.loads(post_data.decode())
            data_provider.fetch_item_line_pool().update_item_line(
                item_line_id, updated_item_line
            )
            data_provider.fetch_item_line_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "item_groups":
            item_group_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_item_group = json.loads(post_data.decode())
            data_provider.fetch_item_group_pool().update_item_group(
                item_group_id, updated_item_group
            )
            data_provider.fetch_item_group_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "item_types":
            item_type_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_item_type = json.loads(post_data.decode())
            data_provider.fetch_item_type_pool().update_item_type(
                item_type_id, updated_item_type
            )
            data_provider.fetch_item_type_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "inventories":
            inventory_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_inventory = json.loads(post_data.decode())
            data_provider.fetch_inventory_pool().update_inventory(
                inventory_id, updated_inventory
            )
            data_provider.fetch_inventory_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "suppliers":
            supplier_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_supplier = json.loads(post_data.decode())
            data_provider.fetch_supplier_pool().update_supplier(
                supplier_id, updated_supplier
            )
            data_provider.fetch_supplier_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "orders":
            paths = len(path)
            match paths:
                case 2:
                    order_id = int(path[1])
                    content_length = int(self.headers["Content-Length"])
                    post_data = self.rfile.read(content_length)
                    updated_order = json.loads(post_data.decode())
                    data_provider.fetch_order_pool().update_order(
                        order_id, updated_order
                    )
                    data_provider.fetch_order_pool().save()
                    self.send_response(200)
                    self.end_headers()
                case 3:
                    if path[2] == "items":
                        order_id = int(path[1])
                        content_length = int(self.headers["Content-Length"])
                        post_data = self.rfile.read(content_length)
                        updated_items = json.loads(post_data.decode())
                        data_provider.fetch_order_pool().update_items_in_order(
                            order_id, updated_items
                        )
                        data_provider.fetch_order_pool().save()
                        self.send_response(200)
                        self.end_headers()
                    else:
                        self.send_response(404)
                        self.end_headers()
                case _:
                    self.send_response(404)
                    self.end_headers()
        elif path[0] == "clients":
            client_id = int(path[1])
            content_length = int(self.headers["Content-Length"])
            post_data = self.rfile.read(content_length)
            updated_client = json.loads(post_data.decode())
            data_provider.fetch_client_pool().update_client(client_id, updated_client)
            data_provider.fetch_client_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "shipments":
            paths = len(path)
            match paths:
                case 2:
                    shipment_id = int(path[1])
                    content_length = int(self.headers["Content-Length"])
                    post_data = self.rfile.read(content_length)
                    updated_shipment = json.loads(post_data.decode())
                    data_provider.fetch_shipment_pool().update_shipment(
                        shipment_id, updated_shipment
                    )
                    data_provider.fetch_shipment_pool().save()
                    self.send_response(200)
                    self.end_headers()
                case 3:
                    if path[2] == "orders":
                        shipment_id = int(path[1])
                        content_length = int(self.headers["Content-Length"])
                        post_data = self.rfile.read(content_length)
                        updated_orders = json.loads(post_data.decode())
                        data_provider.fetch_order_pool().update_orders_in_shipment(
                            shipment_id, updated_orders
                        )
                        data_provider.fetch_order_pool().save()
                        self.send_response(200)
                        self.end_headers()
                    elif path[2] == "items":
                        shipment_id = int(path[1])
                        content_length = int(self.headers["Content-Length"])
                        post_data = self.rfile.read(content_length)
                        updated_items = json.loads(post_data.decode())
                        data_provider.fetch_shipment_pool().update_items_in_shipment(
                            shipment_id, updated_items
                        )
                        data_provider.fetch_shipment_pool().save()
                        self.send_response(200)
                        self.end_headers()
                    elif path[2] == "commit":
                        pass
                    else:
                        self.send_response(404)
                        self.end_headers()
                case _:
                    self.send_response(404)
                    self.end_headers()
        else:
            self.send_response(404)
            self.end_headers()

    def do_PUT(self):
        api_key = self.headers.get("API_KEY")
        user = auth_provider.get_user(api_key)
        if user == None:
            self.send_response(401)
            self.end_headers()
        else:
            try:
                path = self.path.split("/")
                if len(path) > 3 and path[1] == "api" and path[2] == "v1":
                    self.handle_put_version_1(path[3:], user)
            except Exception:
                self.send_response(500)
                self.end_headers()

    def handle_delete_version_1(self, path, user):
        if not auth_provider.has_access(user, path, "delete"):
            self.send_response(403)
            self.end_headers()
            return
        if path[0] == "warehouses":
            warehouse_id = int(path[1])
            data_provider.fetch_warehouse_pool().remove_warehouse(warehouse_id)
            data_provider.fetch_warehouse_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "locations":
            location_id = int(path[1])
            data_provider.fetch_location_pool().remove_location(location_id)
            data_provider.fetch_location_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "transfers":
            transfer_id = int(path[1])
            data_provider.fetch_transfer_pool().remove_transfer(transfer_id)
            data_provider.fetch_transfer_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "items":
            item_id = path[1]
            data_provider.fetch_item_pool().remove_item(item_id)
            data_provider.fetch_item_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "item_lines":
            item_line_id = int(path[1])
            data_provider.fetch_item_line_pool().remove_item_line(item_line_id)
            data_provider.fetch_item_line_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "item_groups":
            item_group_id = int(path[1])
            data_provider.fetch_item_group_pool().remove_item_group(item_group_id)
            data_provider.fetch_item_group_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "item_types":
            item_type_id = int(path[1])
            data_provider.fetch_item_type_pool().remove_item_type(item_type_id)
            data_provider.fetch_item_type_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "inventories":
            inventory_id = int(path[1])
            data_provider.fetch_inventory_pool().remove_inventory(inventory_id)
            data_provider.fetch_inventory_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "suppliers":
            supplier_id = int(path[1])
            data_provider.fetch_supplier_pool().remove_supplier(supplier_id)
            data_provider.fetch_supplier_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "orders":
            order_id = int(path[1])
            data_provider.fetch_order_pool().remove_order(order_id)
            data_provider.fetch_order_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "clients":
            client_id = int(path[1])
            data_provider.fetch_client_pool().remove_client(client_id)
            data_provider.fetch_client_pool().save()
            self.send_response(200)
            self.end_headers()
        elif path[0] == "shipments":
            shipment_id = int(path[1])
            data_provider.fetch_shipment_pool().remove_shipment(shipment_id)
            data_provider.fetch_shipment_pool().save()
            self.send_response(200)
            self.end_headers()
        else:
            self.send_response(404)
            self.end_headers()

    def do_DELETE(self):
        """Handle DELETE requests."""
        path_parts = self.path.split("/")
        if len(path_parts) == 5 and path_parts[1] == "api" and path_parts[2] == "v1":
            # Extract resource type and ID
            resource_type = path_parts[3]
            try:
                resource_id = int(path_parts[4])
            except ValueError:
                self.send_response(400)
                self.end_headers()
                self.wfile.write(json.dumps({"error": "Invalid resource ID"}).encode())
                return

            # Check for dry-run mode
            dry_run = "dry_run=true" in self.path

            # Perform or simulate deletion
            if resource_type == "clients":
                response, status_code = clients_instance.remove_client(
                    resource_id, dry_run
                )
            elif resource_type == "warehouses":
                response, status_code = warehouses_instance.remove_warehouse(
                    resource_id, dry_run
                )
            elif resource_type == "locations":
                response, status_code = locations_instance.remove_location(
                    resource_id, dry_run
                )
            elif resource_type == "transfers":
                response, status_code = transfers_instance.remove_transfer(
                    resource_id, dry_run
                )
            elif resource_type == "items":
                response, status_code = items_instance.remove_item(resource_id, dry_run)
            elif resource_type == "item_lines":
                response, status_code = item_lines_instance.remove_item_line(
                    resource_id, dry_run
                )
            elif resource_type == "item_groups":
                response, status_code = item_groups_instance.remove_item_group(
                    resource_id, dry_run
                )
            elif resource_type == "item_types":
                response, status_code = item_types_instance.remove_item_type(
                    resource_id, dry_run
                )
            elif resource_type == "inventories":
                response, status_code = inventories_instance.remove_inventory(
                    resource_id, dry_run
                )
            elif resource_type == "suppliers":
                response, status_code = suppliers_instance.remove_supplier(
                    resource_id, dry_run
                )
            elif resource_type == "orders":
                response, status_code = orders_instance.remove_order(
                    resource_id, dry_run
                )
            elif resource_type == "shipments":
                response, status_code = shipments_instance.remove_shipment(
                    resource_id, dry_run
                )
            else:
                self.send_response(404)
                self.end_headers()
                return

            self.send_response(status_code)
            self.send_header("Content-type", "application/json")
            self.end_headers()
            self.wfile.write(json.dumps(response).encode())
        else:
            self.send_response(404)
            self.end_headers()


if __name__ == "__main__":
    PORT = 3000
    with socketserver.TCPServer(("", PORT), ApiRequestHandler) as httpd:
        auth_provider.init()
        data_provider.init()
        notification_processor.start()
        print(f"Welcome to CargoHub! (Serving on port {PORT})")
        print("Waiting for requests...")
        httpd.serve_forever()
