export interface SellableOption {
  id: string;
  price: number;
  description: string | null;
  imageUrl: string | null;
  isMarkedAvailable: boolean;
  isActive: boolean;
  isAvailable: boolean;
  isVisibleToPublic: boolean;
}

export interface Product {
  id: string;
  name: string;
  description: string | null;
  imageUrl: string | null;
  hasImage: boolean;
  categoryId: string;
  isActive: boolean;
  isAvailable: boolean;
  isVisibleToPublic: boolean;
  options: SellableOption[];
}

export interface OptionInput {
  price: number;
  isMarkedAvailable: boolean;
  description: string | null;
  imageUrl: string | null;
}

export interface CreateProductInput {
  name: string;
  description: string | null;
  imageUrl: string | null;
  categoryId: string;
  options: OptionInput[];
}

export interface UpdateProductInput {
  name: string;
  description: string | null;
  imageUrl: string | null;
  categoryId: string;
}
