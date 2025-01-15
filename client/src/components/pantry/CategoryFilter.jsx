import { useEffect, useState } from "react";
import { getCategories } from "../../managers/categoryManager"; // Create this manager to fetch categories

export const CategoryDropdown = ({ onCategorySelect }) => {
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState("");

  useEffect(() => {
    getCategories()
      .then((data) => setCategories(data))
      .catch((err) => console.error("Error fetching categories:", err));
  }, []);

  const handleChange = (event) => {
    const categoryId = event.target.value;
    setSelectedCategory(categoryId);
    onCategorySelect(categoryId); // Pass the selected category ID to the parent
  };

  return (
    <select
      value={selectedCategory}
      onChange={handleChange}
      className="form-select"
    >
      <option value="">-- Select a Category --</option>
      {categories.map((category) => (
        <option key={category.id} value={category.id}>
          {category.name}
        </option>
      ))}
    </select>
  );
};
