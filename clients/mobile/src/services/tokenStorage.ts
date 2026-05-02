import * as SecureStore from 'expo-secure-store'

const TOKEN_KEY = 'whatleft_access_token'

export const tokenStorage = {
  async get(): Promise<string | null> {
    return SecureStore.getItemAsync(TOKEN_KEY)
  },
  async set(token: string): Promise<void> {
    await SecureStore.setItemAsync(TOKEN_KEY, token)
  },
  async remove(): Promise<void> {
    await SecureStore.deleteItemAsync(TOKEN_KEY)
  },
}
