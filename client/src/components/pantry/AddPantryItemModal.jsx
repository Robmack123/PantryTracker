import { useState } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  Form,
  FormGroup,
  Label,
  Input,
} from "reactstrap";
import { addOrUpdatePantryItem } from "../../managers/pantryItemManager";

export const AddPantryItemModal = ({ isOpen, toggle, refreshPantryItems }) => {
  const [itemName, setItemName] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [categories, setCategories] = useState([]);
  const [error, setError] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    setError("");

    const newItem = {
      name: itemName,
      quantity: parseInt(quantity, 10),
      categoryIds: categories.map(Number), // Ensure IDs are integers
    };

    addOrUpdatePantryItem(newItem)
      .then(() => {
        refreshPantryItems(); // Refresh the pantry list
        toggle(); // Close the modal
      })
      .catch((err) => {
        console.error("Error adding pantry item:", err);
        setError("Failed to add pantry item.");
      });
  };

  const handleCategoryChange = (event) => {
    const options = event.target.options;
    const selectedCategories = [];
    for (let i = 0; i < options.length; i++) {
      if (options[i].selected) {
        selectedCategories.push(options[i].value);
      }
    }
    setCategories(selectedCategories);
  };

  return (
    <Modal isOpen={isOpen} toggle={toggle}>
      <ModalHeader toggle={toggle}>Add New Pantry Item</ModalHeader>
      <ModalBody>
        {error && <p className="text-danger">{error}</p>}
        <Form onSubmit={handleSubmit}>
          <FormGroup>
            <Label for="itemName">Item Name</Label>
            <Input
              type="text"
              id="itemName"
              value={itemName}
              onChange={(e) => setItemName(e.target.value)}
              required
            />
          </FormGroup>
          <FormGroup>
            <Label for="quantity">Quantity</Label>
            <Input
              type="number"
              id="quantity"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
              min="1"
              required
            />
          </FormGroup>
          <FormGroup>
            <Label for="categories">Categories</Label>
            <Input
              type="select"
              id="categories"
              multiple
              onChange={handleCategoryChange}
            >
              {/* Replace with dynamic categories */}
              <option value="1">Dairy</option>
              <option value="2">Snacks</option>
              <option value="3">Beverages</option>
              <option value="4">Produce</option>
            </Input>
          </FormGroup>
          <ModalFooter>
            <Button color="primary" type="submit">
              Add Item
            </Button>
            <Button color="secondary" onClick={toggle}>
              Cancel
            </Button>
          </ModalFooter>
        </Form>
      </ModalBody>
    </Modal>
  );
};
