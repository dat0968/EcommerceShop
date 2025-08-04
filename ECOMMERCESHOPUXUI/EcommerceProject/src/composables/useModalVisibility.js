import { ref, watch } from 'vue';
import { useRoute } from 'vue-router';

/**
 * Composable để quản lý việc hiển thị của một component dựa trên route hiện tại.
 *
 * @param {object} [config={}] - Đối tượng cấu hình.
 * @param {string[]} [config.include] - Một mảng các route name NƠI MÀ component SẼ được hiển thị. Ưu tiên hơn 'exclude'.
 * @param {string[]} [config.exclude] - Một mảng các route name NƠI MÀ component SẼ BỊ ẨN. Sẽ được dùng nếu 'include' không được cung cấp.
 * @returns {{isModalVisible: import('vue').Ref<boolean>}} - Trả về một object chứa trạng thái isModalVisible (reactive).
 */
export function useModalVisibility(config = {}) {
  const { include, exclude } = config;
  const route = useRoute();
  const isModalVisible = ref(false);

  const updateVisibility = (currentRoute) => {
    if (!currentRoute || !currentRoute.name) {
      isModalVisible.value = false;
      return;
    }

    const routeName = currentRoute.name;

    // Ưu tiên 1: Nếu có danh sách 'include', component chỉ hiển thị khi route nằm trong danh sách này.
    if (Array.isArray(include) && include.length > 0) {
      isModalVisible.value = include.includes(routeName);
      return;
    }

    // Ưu tiên 2: Nếu có danh sách 'exclude', component sẽ hiển thị ở mọi nơi TRỪ những route trong danh sách.
    if (Array.isArray(exclude)) {
      isModalVisible.value = !exclude.includes(routeName);
      return;
    }

    // Mặc định: Nếu không có cấu hình 'include' hay 'exclude', component sẽ luôn hiển thị.
    isModalVisible.value = true;
  };

  // Theo dõi sự thay đổi của route và cập nhật trạng thái
  watch(
    () => route.name,
    (newRouteName) => {
      updateVisibility({ name: newRouteName });
    },
    { immediate: true, deep: true }
  );

  return {
    isModalVisible,
  };
}