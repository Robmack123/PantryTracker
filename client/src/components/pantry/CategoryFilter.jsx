import { useState, useEffect } from "react";
import Select from "react-select";
import { getCategories } from "../../managers/categoryManager";

export const CategoryDropdown = ({
  onCategorySelect,
  selectedCategories = [],
}) => {
  const [categories, setCategories] = useState([]);

  // Fetch categories on component mount
  useEffect(() => {
    getCategories()
      .then((data) =>
        setCategories(
          data.map((category) => ({ value: category.id, label: category.name }))
        )
      )
      .catch((err) => console.error("Error fetching categories:", err));
  }, []);

  const handleChange = (selectedOptions) => {
    const selectedIds = selectedOptions
      ? selectedOptions.map((option) => option.value)
      : [];
    onCategorySelect(selectedIds); // Pass selected IDs to parent
  };

  return (
    <Select
      options={categories} // Dropdown options
      isMulti // Enable multi-selection
      value={categories.filter((category) =>
        selectedCategories.includes(category.value)
      )} // Show selected categories
      onChange={handleChange} // Handle selection change
      placeholder="Select categories..."
      closeMenuOnSelect={false} // Keep menu open after selection
      menuPlacement="auto" // Adjust dropdown placement dynamically
    />
  );
};
