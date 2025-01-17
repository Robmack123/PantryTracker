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
    onCategorySelect(selectedIds);
  };

  return (
    <Select
      options={categories}
      isMulti
      value={categories.filter((category) =>
        selectedCategories.includes(category.value)
      )}
      onChange={handleChange}
      placeholder="Select categories..."
      closeMenuOnSelect={false}
      menuPlacement="auto"
    />
  );
};
