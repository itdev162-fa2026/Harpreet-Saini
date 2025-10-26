// ProductList.jsx
import React, { useState, useEffect } from 'react';
import ProductCard from './ProductCard';
import './ProductList.css';
import { getProducts } from '../services/api'; // Assuming you've created an API call

function ProductList() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        const data = await getProducts();  // Fetch products from API
        setProducts(data);
      } catch (err) {
        setError('Failed to load products. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    fetchProducts();
  }, []);

  if (loading) return <div className="loading">Loading products...</div>;
  if (error) return <div className="error">{error}</div>;
  if (products.length === 0) return <div className="empty-state">No products available</div>;

  return (
    <div className="product-list-container">
      <h1>Our Products</h1>
      <div className="product-grid">
        {products.map(p => <ProductCard key={p.id} product={p} />)}
      </div>
    </div>
  );
}

export default ProductList;
