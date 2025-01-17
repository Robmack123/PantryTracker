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
  updatePantryItemQuantity,
  deletePantryItem,
  toggleMonitorLowStock,
} from "../../managers/pantryItemManager";

export const ProductDetailsModal = ({
  isOpen,
  toggle,
  product,
  refreshPantryItems,
}) => {
  const [quantity, setQuantity] = useState(product.quantity);
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
    const dto = { quantity };

    updatePantryItemQuantity(product.id, dto)
      .then(() => {
        refreshPantryItems();
        toggle();
      })
      .catch((err) => {
        console.error("Error updating quantity:", err);
        setError("Failed to update the quantity. Please try again.");
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
        <p>
          <strong>Item Name:</strong> {product.name}
        </p>
        <p>
          <strong>Added On:</strong>{" "}
          {new Date(product.updatedAt).toLocaleString()}
        </p>
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
