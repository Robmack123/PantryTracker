const apiUrl = "/api/pantryitem";

export const getPantryItems = (page = 1, pageSize = 10) => {
  return fetch(`${apiUrl}?page=${page}&pageSize=${pageSize}`, {
    method: "GET",
    credentials: "include", // Ensures the user's session cookie is sent with the request
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch pantry items.");
    }
    return res.json();
  });
};

export const getPantryItemsByCategory = (
  categoryIds,
  page = 1,
  pageSize = 10
) => {
  const queryString = categoryIds.map((id) => `categoryIds=${id}`).join("&");
  return fetch(
    `${apiUrl}/by-category?${queryString}&page=${page}&pageSize=${pageSize}`,
    {
      method: "GET",
      credentials: "include", // Ensures cookies are sent with the request
      headers: {
        "Content-Type": "application/json",
      },
    }
  ).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch pantry items by category.");
    }
    return res.json();
  });
};

export const addOrUpdatePantryItem = (pantryItem) => {
  return fetch(`${apiUrl}`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(pantryItem),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to add or update pantry item.");
    }
    return res.json();
  });
};

export const updatePantryItemQuantity = (itemId, dto) => {
  return fetch(`${apiUrl}/${itemId}/quantity`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(dto),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to update quantity.");
    }
    return res.json();
  });
};
