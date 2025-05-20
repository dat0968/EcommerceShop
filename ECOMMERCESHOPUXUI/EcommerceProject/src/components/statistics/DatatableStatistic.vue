<template>
  <code>{{ data }}</code>
</template>

<script>
import DatatableStatisticsResponse from '@/models/dtos/statisticsDtos/datatableStatisticsResponse'
export default {
  name: 'DatatableStatistic',
  components: {},
  props: {
    data: {
      type: DatatableStatisticsResponse,
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
