import { TouchableOpacity, Text, StyleSheet } from 'react-native'

interface Props {
  label: string
  selected?: boolean
  size?: 'sm' | 'xs'
  onPress?: () => void
}

export default function TagChip({ label, selected = false, size = 'sm', onPress }: Props) {
  const isXs = size === 'xs'
  return (
    <TouchableOpacity
      onPress={onPress}
      disabled={!onPress}
      activeOpacity={0.7}
      style={[
        styles.chip,
        isXs && styles.chipXs,
        selected && styles.selected,
      ]}
    >
      <Text style={[styles.text, isXs && styles.textXs, selected && styles.textSelected]}>
        {label}
      </Text>
    </TouchableOpacity>
  )
}

const styles = StyleSheet.create({
  chip: {
    paddingHorizontal: 12,
    paddingVertical: 5,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: '#ddd',
    backgroundColor: '#f5f5f5',
  },
  chipXs: { paddingHorizontal: 7, paddingVertical: 2 },
  selected: { backgroundColor: '#6200ea', borderColor: '#6200ea' },
  text: { fontSize: 13, color: '#555' },
  textXs: { fontSize: 11 },
  textSelected: { color: '#fff', fontWeight: '600' },
})
