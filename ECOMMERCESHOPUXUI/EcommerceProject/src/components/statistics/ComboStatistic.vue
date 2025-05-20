<template>
  <code>{{ data }}</code>
</template>

<script>
import ComboStatisticsResponse from '@/models/dtos/statisticsDtos/comboStatisticsResponse'
export default {
  name: 'ComboStatistic',
  components: {},
  props: {
    data: {
      type: ComboStatisticsResponse,
      required: true,
    },
  },
  data() {
    return {}
  },
  computed: {},
  watch: {},
  async mounted() {},
  methods: {},
}
</script>

<style scoped></style>
