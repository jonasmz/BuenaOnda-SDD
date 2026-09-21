export interface Characteristic {
  id: string;
  name: string;
}

export interface SellableOption {
  id: string;
  values: Record<string, string>;
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
  characteristics: Characteristic[];
  options: SellableOption[];
}

export interface OptionInput {
  values: Record<string, string>;
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
  characteristics: string[];
  options: OptionInput[];
}

export interface UpdateProductInput {
  name: string;
  description: string | null;
  imageUrl: string | null;
  categoryId: string;
}
