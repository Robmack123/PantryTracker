import { useState } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  FormGroup,
  Label,
  Input,
} from "reactstrap";
import {
  updatePantryItemDetails, // New function to update the name and LowStockThreshold
  // updatePantryItemQuantity,
  deletePantryItem,
  toggleMonitorLowStock,
} from "../../managers/pantryItemManager";

export const ProductDetailsModal = ({
  isOpen,
  toggle,
  product,
  refreshPantryItems,
}) => {
  const [name, setName] = useState(product.name); // New state for item name
  const [quantity, setQuantity] = useState(product.quantity);
  const [lowStockThreshold, setLowStockThreshold] = useState(
    product.lowStockThreshold || 2
  ); // New state for LowStockThreshold
  const [monitorLowStock, setMonitorLowStock] = useState(
    product.monitorLowStock
  );
  const [error, setError] = useState("");

  const handleUpdateQuantity = (change) => {
    const newQuantity = quantity + change;
    if (newQuantity >= 0) {
      setQuantity(newQuantity);
    }
  };

  const handleSave = () => {
    setError("");

    const dto = { name, quantity, lowStockThreshold };

    updatePantryItemDetails(product.id, dto) // Use new manager function
      .then(() => {
        refreshPantryItems();
        toggle();
      })
      .catch((err) => {
        console.error("Error updating item details:", err);
        setError("Failed to save changes. Please try again.");
      });
  };

  const handleDelete = () => {
    setError("");
    deletePantryItem(product.id)
      .then(() => {
        refreshPantryItems();
        toggle();
      })
      .catch((err) => {
        console.error("Error deleting pantry item:", err);
        setError("Failed to delete the item. Please try again.");
      });
  };

  const handleToggleMonitorLowStock = () => {
    setError("");
    toggleMonitorLowStock(product.id)
      .then((response) => {
        setMonitorLowStock(response.monitorLowStock);
        refreshPantryItems();
      })
      .catch((err) => {
        console.error("Error toggling MonitorLowStock:", err);
        setError("Failed to update monitoring status. Please try again.");
      });
  };

  return (
    <Modal isOpen={isOpen} toggle={toggle}>
      <ModalHeader toggle={toggle}>Product Details</ModalHeader>
      <ModalBody>
        {error && <p className="text-danger">{error}</p>}
        <FormGroup>
          <Label for="name">Item Name</Label>
          <Input
            type="text"
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </FormGroup>
        <FormGroup>
          <Label for="quantity">Quantity</Label>
          <div className="d-flex align-items-center">
            <Button
              color="secondary"
              size="sm"
              onClick={() => handleUpdateQuantity(-1)}
              className="me-2"
            >
              -
            </Button>
            <Input
              type="number"
              id="quantity"
              value={quantity}
              onChange={(e) =>
                setQuantity(Math.max(0, parseInt(e.target.value, 10) || 0))
              }
              style={{ width: "60px", textAlign: "center" }}
            />
            <Button
              color="secondary"
              size="sm"
              onClick={() => handleUpdateQuantity(1)}
              className="ms-2"
            >
              +
            </Button>
          </div>
        </FormGroup>
        <FormGroup>
          <Label for="lowStockThreshold">Low Stock Threshold</Label>
          <Input
            type="number"
            id="lowStockThreshold"
            value={lowStockThreshold}
            onChange={(e) =>
              setLowStockThreshold(
                Math.max(0, parseInt(e.target.value, 10) || 0)
              )
            }
          />
        </FormGroup>
        <FormGroup check className="mt-3">
          <Label check>
            <Input
              type="checkbox"
              checked={monitorLowStock}
              onChange={handleToggleMonitorLowStock}
            />{" "}
            Monitor for Low Stock
          </Label>
        </FormGroup>
      </ModalBody>
      <ModalFooter>
        <Button color="primary" onClick={handleSave}>
          Save Changes
        </Button>
        <Button color="danger" onClick={handleDelete}>
          Delete Item
        </Button>
        <Button color="secondary" onClick={toggle}>
          Close
        </Button>
      </ModalFooter>
    </Modal>
  );
};
