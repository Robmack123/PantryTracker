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
import { updatePantryItemQuantity } from "../../managers/pantryItemManager";

export const ProductDetailsModal = ({
  isOpen,
  toggle,
  product,
  refreshPantryItems,
}) => {
  const [quantity, setQuantity] = useState(product.quantity);
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
        refreshPantryItems(); // Refresh the main list
        toggle(); // Close the modal
      })
      .catch((err) => {
        console.error("Error updating quantity:", err);
        setError("Failed to update the quantity. Please try again.");
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
      </ModalBody>
      <ModalFooter>
        <Button color="primary" onClick={handleSave}>
          Save Changes
        </Button>
        <Button color="secondary" onClick={toggle}>
          Close
        </Button>
      </ModalFooter>
    </Modal>
  );
};
